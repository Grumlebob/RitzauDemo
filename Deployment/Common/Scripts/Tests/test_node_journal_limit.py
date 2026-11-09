"""The LocalSingleNode setup bounds the system journal on an existing host."""
from pathlib import Path
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[4]
ROLE = ROOT / "Deployment/Common/ansible/roles/mint_base/tasks/main.yml"


class NodeJournalLimitTests(unittest.TestCase):
    def setUp(self):
        self.tasks = {task["name"]: task for task in yaml.safe_load(ROLE.read_text())}

    def test_journal_drop_in_bounds_size_and_keeps_free_space(self):
        task = self.tasks["Bound the system journal"]["ansible.builtin.copy"]
        self.assertEqual("/etc/systemd/journald.conf.d/50-grumplates-size.conf", task["dest"])
        lines = task["content"].splitlines()
        self.assertEqual("[Journal]", lines[0])
        self.assertIn("SystemMaxUse=1G", lines)
        self.assertIn("SystemKeepFree=5G", lines)
        self.assertEqual("0644", task["mode"])

    def test_journald_restarts_only_when_the_drop_in_changes(self):
        task = self.tasks["Apply the journal bound"]
        self.assertEqual({"name": "systemd-journald", "state": "restarted"}, task["ansible.builtin.systemd"])
        self.assertEqual("mint_base_journal_limit.changed", task["when"])
        self.assertEqual("mint_base_journal_limit", self.tasks["Bound the system journal"]["register"])

    def test_single_node_setup_applies_the_shared_base_role(self):
        playbook = "Deployment/LocalSingleNode/ansible/playbooks/PrepareSingleNode.yml"
        plays = yaml.safe_load((ROOT / playbook).read_text())
        roles = [role if isinstance(role, str) else role.get("role") for play in plays for role in play.get("roles", [])]
        self.assertIn("mint_base", roles, playbook)

    def test_single_node_bootstrap_skips_dist_upgrade_on_existing_host(self):
        playbook = yaml.safe_load((ROOT / "Deployment/LocalSingleNode/ansible/playbooks/PrepareSingleNode.yml").read_text())[0]
        self.assertIs(playbook["vars"]["mint_base_upgrade_packages"], False)
        self.assertEqual("mint_base_upgrade_packages | default(true) | bool",
                         self.tasks["Upgrade installed packages"]["when"])

    def test_docker_role_installs_missing_packages_without_requesting_latest(self):
        tasks = yaml.safe_load((ROOT / "Deployment/Common/ansible/roles/docker/tasks/main.yml").read_text())
        task = next(item for item in tasks if item["name"] == "Install Docker Engine and Compose plugin")
        self.assertEqual("present", task["ansible.builtin.apt"]["state"])


if __name__ == "__main__":
    unittest.main()
