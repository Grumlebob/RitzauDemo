from __future__ import annotations

import shutil
import subprocess
import threading
import unittest
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
SCRIPT = ROOT / "Scripts" / "Test-DeployedSite.ps1"
POWERSHELL = shutil.which("pwsh") or shutil.which("powershell")


class AcceptanceHandler(BaseHTTPRequestHandler):
    requests: list[tuple[str, str, str]] = []

    def log_message(self, *_args):
        pass

    def respond(self, status: int, body: bytes = b"", content_type: str = "text/plain", headers=None):
        AcceptanceHandler.requests.append((self.command, self.path, self.headers.get("Cookie", "")))
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(body)))
        for key, value in (headers or {}).items():
            self.send_header(key, value)
        self.end_headers()
        if self.command != "HEAD" and body:
            self.wfile.write(body)

    def do_GET(self):
        path = self.path.split("?", 1)[0]
        if path == "/health/ready":
            self.respond(200, b"Healthy")
        elif path == "/api/books":
            self.respond(401, b'{"error":"unauthorized"}', "application/json")
        elif path == "/Account/Manage":
            self.respond(302, headers={"Location": "/Account/Login"})
        elif path == "/Account/Login":
            self.respond(
                200,
                b'<form><input name="__RequestVerificationToken" value="token"><input name="Input.Email"></form>',
                "text/html",
            )
        elif path == "/":
            self.respond(200, b'<!doctype html><script src="/_framework/blazor.web.js"></script>', "text/html")
        else:
            self.respond(404, b"Not found")

    def do_HEAD(self):
        self.do_GET()

    def do_POST(self):
        length = int(self.headers.get("Content-Length", "0"))
        if length:
            self.rfile.read(length)
        self.respond(500, b"Writes are forbidden in this fixture")


@unittest.skipUnless(POWERSHELL, "PowerShell is required for deployed-site acceptance tests")
class DeployedSiteAcceptanceTests(unittest.TestCase):
    def run_script(self, *args):
        return subprocess.run(
            [POWERSHELL, "-NoProfile", "-File", str(SCRIPT), *args],
            capture_output=True,
            text=True,
            timeout=120,
            check=False,
        )

    def run_fixture(self, forwarded_proto: str = "https"):
        AcceptanceHandler.requests = []
        server = ThreadingHTTPServer(("127.0.0.1", 0), AcceptanceHandler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            result = self.run_script(
                "-FrontendProfile", "BlazorAuto",
                "-AcceptanceProfile", "BlazorAutoV1",
                "-BaseUrl", f"http://127.0.0.1:{server.server_port}",
                *(("-ForwardedProto", forwarded_proto) if forwarded_proto else ()),
            )
            requests = list(AcceptanceHandler.requests)
        finally:
            server.shutdown()
            thread.join(timeout=5)
            server.server_close()
        return result, requests

    def test_react_profile_is_rejected_before_network_access(self):
        result = self.run_script(
            "-FrontendProfile", "React",
            "-AcceptanceProfile", "ReactPublicV1",
            "-BaseUrl", "http://127.0.0.1:1",
        )
        self.assertNotEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertNotIn("PASS DNS", result.stdout)

    def test_blazor_acceptance_checks_health_api_and_account_routes_without_writes(self):
        result, requests = self.run_fixture()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertIn("PASS health", result.stdout)
        self.assertIn("PASS anonymous-API", result.stdout)
        self.assertIn("PASS account-manage-anonymous", result.stdout)
        self.assertIn("PASS account-login-page", result.stdout)
        self.assertIn("RESULT: PASS", result.stdout)
        paths = {path.split("?", 1)[0] for _method, path, _cookie in requests}
        self.assertTrue({"/health/ready", "/api/books", "/", "/Account/Manage", "/Account/Login"} <= paths)
        self.assertTrue(all(method in {"GET", "HEAD"} for method, _path, _cookie in requests))

    def test_plain_http_skips_secure_login_form_check(self):
        result, requests = self.run_fixture(forwarded_proto="")
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertNotIn("account-login-page", result.stdout)
        self.assertIn("PASS account-manage-anonymous", result.stdout)
        self.assertIn("RESULT: PASS", result.stdout)
        paths = {path.split("?", 1)[0] for _method, path, _cookie in requests}
        self.assertNotIn("/Account/Login", paths)


if __name__ == "__main__":
    unittest.main()
