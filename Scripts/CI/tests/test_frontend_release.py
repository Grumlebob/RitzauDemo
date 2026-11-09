from __future__ import annotations

import hashlib
import json
from pathlib import Path
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[3]
from Scripts.CI.write_release_manifest import write_manifest


class ReleaseManifestFixture:
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.directory = Path(temporary.name) / "staging"
        self.directory.mkdir()
        self.env = {
            "FRONTEND_PROFILE": "BlazorAuto",
            "FRONTEND_ACCEPTANCE_PROFILE": "BlazorAutoV1",
            "APP_IMAGE": "ghcr.io/example/app",
            "GITHUB_SERVER_URL": "https://github.com",
            "GITHUB_REPOSITORY": "example/template",
            "GITHUB_SHA": "a" * 40,
            "GITHUB_RUN_ID": "123",
            "GITHUB_RUN_ATTEMPT": "2",
            "MIGRATION_BUNDLE_NAME": "app-migrate",
            "MIGRATION_RUNTIME": "linux-x64",
        }
        self.bundle = self.directory / "app-migrate"
        self.bundle.write_bytes(b"verified migration bytes")
        self.provenance = {
            "schema_version": 1,
            "repository": "https://github.com/example/template",
            "source_sha": "a" * 40,
            "run_id": "123",
            "run_attempt": "2",
            "bundle_filename": "app-migrate",
            "runtime": "linux-x64",
            "dotnet_sdk_version": "10.0.100",
            "ordered_migration_ids": ["20260101000000_Initial"],
            "bundle_sha256": hashlib.sha256(self.bundle.read_bytes()).hexdigest(),
        }
        self.save_provenance()
        self.digests = [f"{self.env['APP_IMAGE']}@sha256:{'b' * 64}"]
        self.labels = {
            "org.opencontainers.image.source": self.provenance["repository"],
            "org.opencontainers.image.revision": self.env["GITHUB_SHA"],
            "com.grumplates.frontend-profile": "BlazorAuto",
            "com.grumplates.acceptance-profile": "BlazorAutoV1",
            "localcluster.ci.repository": "example/template",
            "localcluster.ci.owner": "ci-build",
            "localcluster.ci.run_id": "123",
            "localcluster.ci.run_attempt": "2",
        }

    def save_provenance(self):
        (self.directory / "migration-provenance.json").write_text(json.dumps(self.provenance), encoding="utf-8")


class ReleaseManifestTests(ReleaseManifestFixture, unittest.TestCase):
    def test_blazor_manifest_binds_image_and_verified_migration_bytes(self):
        manifest = write_manifest(self.directory, self.digests, self.labels, self.env)

        self.assertEqual(2, manifest["schema_version"])
        self.assertEqual("BlazorAuto", manifest["frontend_profile"])
        self.assertEqual("BlazorAutoV1", manifest["acceptance_profile"])
        self.assertEqual(self.provenance["bundle_sha256"], manifest["bundle"]["sha256"])
        self.assertEqual("sha256:" + "b" * 64, manifest["image_digest"])
        self.assertEqual(self.env["APP_IMAGE"] + ":" + "a" * 40, manifest["image_ref"])
        self.assertEqual(json.loads((self.directory / "release-manifest.json").read_text()), manifest)

    def test_react_or_mismatched_acceptance_is_rejected(self):
        self.env.update(FRONTEND_PROFILE="React", FRONTEND_ACCEPTANCE_PROFILE="ReactPublicV1")
        with self.assertRaisesRegex(ValueError, "unsupported frontend profile"):
            write_manifest(self.directory, self.digests, self.labels, self.env)

        self.env.update(FRONTEND_PROFILE="BlazorAuto", FRONTEND_ACCEPTANCE_PROFILE="ReactPublicV1")
        with self.assertRaisesRegex(ValueError, "does not match frontend"):
            write_manifest(self.directory, self.digests, self.labels, self.env)

    def test_missing_mismatched_or_duplicate_registry_digest_fails(self):
        for digests in (None, [], self.digests * 2,
                        ["ghcr.io/example/other@sha256:" + "b" * 64],
                        [self.env["APP_IMAGE"] + "@sha256:bad"]):
            with self.subTest(digests=digests), self.assertRaises(ValueError):
                write_manifest(self.directory, digests, self.labels, self.env)

    def test_every_image_label_is_required_and_bound_to_the_release(self):
        for key in self.labels:
            for value in (None, "incorrect"):
                labels = {**self.labels, key: value}
                with self.subTest(key=key, value=value), self.assertRaises(ValueError):
                    write_manifest(self.directory, self.digests, labels, self.env)

    def test_changed_bundle_is_rejected_without_writing_a_manifest(self):
        self.bundle.write_bytes(b"changed")
        with self.assertRaises(ValueError):
            write_manifest(self.directory, self.digests, self.labels, self.env)
        self.assertFalse((self.directory / "release-manifest.json").exists())

    def test_staging_source_run_runtime_or_filename_mismatch_fails(self):
        for key in ("source_sha", "repository", "run_id", "run_attempt", "runtime", "bundle_filename"):
            original = self.provenance[key]
            self.provenance[key] = "incorrect"
            self.save_provenance()
            with self.subTest(key=key), self.assertRaises(ValueError):
                write_manifest(self.directory, self.digests, self.labels, self.env)
            self.provenance[key] = original

    def test_existing_manifest_is_never_overwritten(self):
        output = self.directory / "release-manifest.json"
        output.write_text("original", encoding="utf-8")
        with self.assertRaises(ValueError):
            write_manifest(self.directory, self.digests, self.labels, self.env)
        self.assertEqual("original", output.read_text())


if __name__ == "__main__":
    unittest.main()
