from __future__ import annotations

import importlib.util
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


REPOSITORY_ROOT = Path(__file__).resolve().parents[3]
RESOLVER_PATH = REPOSITORY_ROOT / "Scripts" / "Frontend" / "resolve_frontend_profile.py"
SPEC = importlib.util.spec_from_file_location("resolve_frontend_profile", RESOLVER_PATH)
assert SPEC and SPEC.loader
resolver = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(resolver)


class FrontendProfileResolverTests(unittest.TestCase):
    def test_blazor_auto_is_the_default_and_only_supported_profile(self) -> None:
        self.assertEqual(resolver.resolve_frontend_profile(), "BlazorAuto")
        self.assertEqual(resolver.resolve_frontend_profile(" BlazorAuto "), "BlazorAuto")
        self.assertEqual(resolver.SUPPORTED_PROFILES, ("BlazorAuto",))

    def test_unsupported_or_empty_override_is_rejected(self) -> None:
        for value in ("", "React", "BlazorAuto\nReact"):
            with self.subTest(value=value), self.assertRaisesRegex(ValueError, "(empty|must be exactly)"):
                resolver.resolve_frontend_profile(value)

    def test_acceptance_profile_is_paired_with_blazor_auto(self) -> None:
        self.assertEqual(resolver.acceptance_profile_for_frontend("BlazorAuto"), "BlazorAutoV1")
        with self.assertRaisesRegex(ValueError, "unsupported frontend profile"):
            resolver.acceptance_profile_for_frontend("React")

    def test_explicit_release_property_is_resolved_from_xml(self) -> None:
        properties = "<Project><PropertyGroup><FrontendProfile>BlazorAuto</FrontendProfile></PropertyGroup></Project>"
        self.assertEqual(resolver.frontend_profile_from_properties(properties), "BlazorAuto")

    def test_legacy_release_profile_file_remains_available_for_rollback(self) -> None:
        legacy_props = "<Project><PropertyGroup><FrontendProfile>$(LegacyProfile)</FrontendProfile></PropertyGroup></Project>"
        with patch.object(resolver, "_git_show", side_effect=[legacy_props, "BlazorAuto\n"]) as git_show:
            profile = resolver.frontend_profile_for_source(REPOSITORY_ROOT, "a" * 40)

        self.assertEqual(profile, "BlazorAuto")
        self.assertEqual(git_show.call_args_list[0].args[2], "Directory.Build.props")
        self.assertEqual(git_show.call_args_list[1].args[2], "frontend-profile.txt")

    def test_current_release_property_does_not_need_a_profile_file(self) -> None:
        properties = "<Project><PropertyGroup><FrontendProfile>BlazorAuto</FrontendProfile></PropertyGroup></Project>"
        with patch.object(resolver, "_git_show", return_value=properties) as git_show:
            profile = resolver.frontend_profile_for_source(REPOSITORY_ROOT, "b" * 40)

        self.assertEqual(profile, "BlazorAuto")
        git_show.assert_called_once()

    def test_cli_emits_frontend_and_acceptance_profiles(self) -> None:
        result = subprocess.run(
            [sys.executable, str(RESOLVER_PATH)], check=False, capture_output=True, text=True
        )
        acceptance = subprocess.run(
            [sys.executable, str(RESOLVER_PATH), "--acceptance-profile"],
            check=False,
            capture_output=True,
            text=True,
        )

        self.assertEqual((result.returncode, result.stdout, result.stderr), (0, "BlazorAuto\n", ""))
        self.assertEqual((acceptance.returncode, acceptance.stdout, acceptance.stderr), (0, "BlazorAutoV1\n", ""))

    def test_source_sha_must_be_full_and_lowercase(self) -> None:
        for value in ("abc", "A" * 40, "1" * 39):
            with self.subTest(value=value), self.assertRaisesRegex(ValueError, "full lowercase 40-character"):
                resolver.frontend_profile_for_source(REPOSITORY_ROOT, value)


class MsBuildFrontendProfileTests(unittest.TestCase):
    def run_profile_target(
        self,
        *,
        override: str | None = None,
        environment_profile: tuple[str, str] | None = None,
    ) -> subprocess.CompletedProcess[str]:
        with tempfile.TemporaryDirectory(prefix="frontend-profile-msbuild-") as directory:
            project_root = Path(directory)
            shutil.copy2(REPOSITORY_ROOT / "Directory.Build.props", project_root / "Directory.Build.props")
            shutil.copy2(REPOSITORY_ROOT / "Directory.Build.targets", project_root / "Directory.Build.targets")
            project_file = project_root / "ProfileProbe.csproj"
            project_file.write_text(
                '<Project Sdk="Microsoft.NET.Sdk">'
                "<PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>"
                "</Project>",
                encoding="utf-8",
            )

            command = ["dotnet", "msbuild", str(project_file), "-nologo", "-target:ValidateFrontendProfile"]
            if override is not None:
                command.append(f"-property:FrontendProfile={override}")
            environment = os.environ.copy()
            environment.pop("FrontendProfile", None)
            environment.pop("FRONTENDPROFILE", None)
            environment.pop("frontendprofile", None)
            if environment_profile is not None:
                environment[environment_profile[0]] = environment_profile[1]
            return subprocess.run(
                command,
                cwd=REPOSITORY_ROOT,
                check=False,
                capture_output=True,
                text=True,
                env=environment,
            )

    def test_msbuild_accepts_the_blazor_auto_default(self) -> None:
        result = self.run_profile_target()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_msbuild_rejects_react_override(self) -> None:
        result = self.run_profile_target(override="React")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Invalid frontend profile", result.stdout + result.stderr)

    def test_msbuild_rejects_environment_profile(self) -> None:
        result = self.run_profile_target(environment_profile=("FrontendProfile", "React"))
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("environment variable is not supported", result.stdout + result.stderr)


if __name__ == "__main__":
    unittest.main()
