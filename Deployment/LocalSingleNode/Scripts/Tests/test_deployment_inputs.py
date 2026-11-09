from __future__ import annotations

import importlib.util
import json
import os
from pathlib import Path
import stat
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest import mock

sys.path.insert(0, str(Path(__file__).parent))
from test_settings_fixture import fixture_settings

SCRIPT = Path(__file__).resolve().parents[1] / "resolve-deployment.py"
SPEC = importlib.util.spec_from_file_location("deployment", SCRIPT)
assert SPEC and SPEC.loader
deployment = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(deployment)


class DeploymentInputTests(unittest.TestCase):
    def setUp(self):
        self.config = fixture_settings()
        self.release = {
            "app_image": "ghcr.io/example/books",
            "migration_bundle_name": "books-migrate",
            "migration_artifact_name": "books-migrate-linux-x64",
        }
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)

    def test_primary_settings_bind_to_the_selected_blazor_release(self):
        values = deployment.resolve(
            "BlazorAuto", "BlazorAutoV1", self.release, self.config, self.directory
        )
        self.assertEqual(values["APP_NAME"], self.config["app_name"])
        self.assertEqual(values["APP_IMAGE"], self.release["app_image"])
        self.assertEqual(values["DEPLOY_ROOT"], self.config["deploy_root"])
        variables = json.loads(Path(values["DEPLOYMENT_VARS_FILE"]).read_text())
        self.assertEqual(variables["app_name"], self.config["app_name"])
        self.assertEqual(variables["app_image"], self.release["app_image"])
        if os.name == "posix":
            self.assertEqual(stat.S_IMODE(Path(values["DEPLOYMENT_VARS_FILE"]).stat().st_mode), 0o600)

    def test_temporary_directory_must_remain_inside_runner_temp(self):
        run_scoped = self.directory / "deployment-123-1"
        with mock.patch.dict(os.environ, {
            "RUNNER_TEMP": str(self.directory),
            "DEPLOYMENT_TEMP_DIR": str(run_scoped),
        }, clear=False):
            result = deployment.resolve_temporary_directory()
        self.assertEqual(result, run_scoped.resolve())
        if os.name == "posix":
            self.assertEqual(stat.S_IMODE(result.stat().st_mode), 0o700)
        with tempfile.TemporaryDirectory() as outside:
            with mock.patch.dict(os.environ, {
                "RUNNER_TEMP": str(self.directory),
                "DEPLOYMENT_TEMP_DIR": str(Path(outside) / "outside"),
            }, clear=False), self.assertRaises(ValueError):
                deployment.resolve_temporary_directory()

    def test_owner_marker_binds_primary_repository_and_settings_roots(self):
        marker = self.directory / "localsinglenode/apps/books.json"
        marker.parent.mkdir(parents=True)
        source = "https://github.com/example/template"
        identity = {
            "source_repo_url": source,
            "deploy_root": self.config["deploy_root"],
            "backup_root": self.config["backup_root"],
        }
        protected = SimpleNamespace(st_mode=stat.S_IFREG | 0o644, st_uid=0)
        with mock.patch.object(Path, "lstat", return_value=protected):
            marker.write_text(json.dumps(identity), encoding="utf-8")
            deployment.verify_owned(self.config, source, self.directory)
            marker.write_text(json.dumps({**identity, "deploy_root": "/opt/foreign"}), encoding="utf-8")
            with self.assertRaises(ValueError):
                deployment.verify_owned(self.config, source, self.directory)


if __name__ == "__main__":
    unittest.main()
