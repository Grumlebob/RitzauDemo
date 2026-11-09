"""Stable settings for offline tests, independent of a clone's app identity."""
from pathlib import Path
import unittest
import sys
import tempfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'lib'))
from ls_settings import settings as validate_settings

SETTINGS_YAML = """app_name: books
app_port: 8080
postgres_port: 5432
redis_port: 6379
deploy_root: /opt/books
docker_subnet: 172.30.10.0/24
lan_http_port: 80
lan_hostnames: []
backup_root: /opt/books-backups
backup_keep_days: 7
observability_enabled: false
"""


def write_settings(path):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(SETTINGS_YAML)


def fixture_settings():
    # Exercise the production parser and validation rather than bypassing them.
    with tempfile.TemporaryDirectory() as directory:
        path = Path(directory) / 'settings.yml'
        write_settings(path)
        return validate_settings(path)


class AccountSettingsTests(unittest.TestCase):
    def test_legacy_settings_default_seeded_accounts_off(self):
        values = fixture_settings()
        self.assertFalse(values["accounts_admin_seed_enabled"])
        self.assertFalse(values["accounts_user_seed_enabled"])

    def test_known_seed_settings_accept_synthetic_opt_in(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "settings.yml"
            path.write_text(SETTINGS_YAML + """accounts_admin_seed_enabled: true
accounts_admin_seed_email: admin@example.invalid
accounts_user_seed_enabled: true
accounts_user_seed_email: user@example.invalid
""")
            values = validate_settings(path)
        self.assertTrue(values["accounts_admin_seed_enabled"])
        self.assertTrue(values["accounts_user_seed_enabled"])
        self.assertEqual("user@example.invalid", values["accounts_user_seed_email"])

    def test_removed_account_settings_are_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "settings.yml"
            for unknown in (
                "accounts_unreviewed: true\n",
                "accounts_registration_enabled: false\n",
                "accounts_email_host: smtp.example.invalid\n",
            ):
                path.write_text(SETTINGS_YAML + unknown)
                with self.subTest(unknown=unknown), self.assertRaisesRegex(ValueError, "missing or unknown"):
                    validate_settings(path)
