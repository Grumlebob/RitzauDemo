from __future__ import annotations

import json
import os
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path
import xml.etree.ElementTree as ET

from Scripts.CI.validate_dotnet_test_results import summarize_results, validate_results

OPT_IN = "Set RUN_E2E=1 to run the packaged Blazor browser smoke."


class DotnetTestResultValidationTests(unittest.TestCase):
    def write_trx(self, outcomes: tuple[str, ...], skip_message: str | None = OPT_IN) -> Path:
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        path = Path(temporary.name) / "results.trx"
        result_rows = "".join(
            f'<UnitTestResult testName="case-{index}" outcome="{outcome}">'
            + (
                f"<Output><ErrorInfo><Message>{skip_message}</Message></ErrorInfo></Output>"
                if outcome == "NotExecuted" and skip_message is not None
                else ""
            )
            + "</UnitTestResult>"
            for index, outcome in enumerate(outcomes)
        )
        path.write_text(
            "<TestRun xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\">"
            f"<Results>{result_rows}</Results></TestRun>",
            encoding="utf-8",
        )
        return path

    def test_passes_with_one_explicitly_opt_in_browser_smoke(self) -> None:
        summary = validate_results(self.write_trx(("Passed", "Passed", "NotExecuted")))
        self.assertEqual((3, 2, 0, 1), (summary.total, summary.passed, summary.failed, summary.skipped))

    def test_unexplained_skips_fail(self) -> None:
        for message in (None, "", "Flaky on CI", "Skipped for now"):
            with self.subTest(message=message), self.assertRaisesRegex(ValueError, "no explicit opt-in reason"):
                validate_results(self.write_trx(("Passed", "NotExecuted"), skip_message=message))

    def test_failed_or_unexpected_outcomes_fail(self) -> None:
        for outcome in ("Failed", "Inconclusive"):
            with self.subTest(outcome=outcome), self.assertRaisesRegex(ValueError, "unexpected test outcomes"):
                validate_results(self.write_trx(("Passed", outcome)))

    def test_no_passed_tests_or_no_results_fail(self) -> None:
        with self.assertRaisesRegex(ValueError, "no test passed"):
            validate_results(self.write_trx(("NotExecuted",)))
        with self.assertRaisesRegex(ValueError, "contains no test results"):
            summarize_results(self.write_trx(()))


REPO_ROOT = Path(__file__).resolve().parents[3]
DOTNET = r'''#!/usr/bin/env python3
import json
import os
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

args = sys.argv[1:]
with open(os.environ["DOTNET_CALLS"], "a") as calls:
    calls.write(json.dumps(args) + "\n")
if args[0] != "test":
    sys.exit(0)
if "-p:FrontendProfile=BlazorAuto" not in args:
    sys.exit("unexpected frontend profile")
if "--filter" in args:
    sys.exit("the CRUD suite must run as one unfiltered test pass")
root = ET.Element("TestRun")
results = ET.SubElement(root, "Results")
for identity, outcome, message in json.loads(os.environ["DOTNET_CASES"]):
    result = ET.SubElement(results, "UnitTestResult", testName=identity, outcome=outcome)
    if message:
        ET.SubElement(ET.SubElement(ET.SubElement(result, "Output"), "ErrorInfo"), "Message").text = message
destination = Path(args[args.index("--results-directory") + 1])
destination.mkdir(parents=True, exist_ok=True)
name = args[args.index("--logger") + 1].split("LogFileName=", 1)[1]
ET.ElementTree(root).write(destination / name, encoding="utf-8")
'''


@unittest.skipUnless(os.name == "posix" and shutil.which("bash"), "requires Linux/WSL bash")
class CrudValidationScriptTests(unittest.TestCase):
    def run_gate(self, cases):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        root = Path(temporary.name)
        executable = root / "dotnet"
        executable.write_text(DOTNET, encoding="utf-8")
        executable.chmod(0o755)
        calls = root / "calls.jsonl"
        results = root / "results"
        process = subprocess.run(
            ["bash", str(REPO_ROOT / "Scripts/CI/validate-frontend-profiles.sh"), str(results)],
            env=dict(os.environ, PATH=f"{root}{os.pathsep}{os.environ['PATH']}",
                     DOTNET_CASES=json.dumps(cases), DOTNET_CALLS=str(calls)),
            capture_output=True, text=True, check=False,
        )
        return process, [json.loads(line) for line in calls.read_text().splitlines()], results

    def good_cases(self):
        return [
            ("RitzauDemo.Test.Features.Books.Unit.BookRequestValidationTests.Valid", "Passed", ""),
            ("RitzauDemo.Test.Features.Books.Integration.BooksCrudIntegrationTests.OwnerCrud", "Passed", ""),
            ("RitzauDemo.Test.E2E.AppShell.RenderModeE2ETests.PackagedSmoke", "NotExecuted", OPT_IN),
        ]

    def test_restores_builds_and_runs_one_blazor_crud_suite(self):
        process, calls, results = self.run_gate(self.good_cases())
        self.assertEqual(0, process.returncode, process.stdout + process.stderr)
        self.assertEqual(["restore", "build", "test"], [call[0] for call in calls])
        test_call = calls[-1]
        self.assertIn("RitzauDemo.Test/RitzauDemo.Test.csproj", test_call[1])
        self.assertIn("-p:FrontendProfile=BlazorAuto", test_call)
        self.assertNotIn("--filter", test_call)
        self.assertTrue((results / "BlazorAuto" / "books-crud.trx").is_file())

    def test_empty_discovery_unexplained_skip_and_failure_fail(self):
        cases = (
            [],
            [("crud", "NotExecuted", "Skipped without an opt-in")],
            [("crud", "Failed", "assertion failed")],
        )
        expected_messages = ("contains no test results", "no explicit opt-in reason", "unexpected test outcomes")
        for rows, message in zip(cases, expected_messages, strict=True):
            with self.subTest(rows=rows):
                process, _, _ = self.run_gate(rows)
                self.assertNotEqual(0, process.returncode)
                self.assertIn(message, process.stderr)


if __name__ == "__main__":
    unittest.main()
