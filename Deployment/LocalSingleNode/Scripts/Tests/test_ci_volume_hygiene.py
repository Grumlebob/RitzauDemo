from pathlib import Path
from unittest import TestCase

ROOT = Path(__file__).resolve().parents[4]
CI_SMOKE = ROOT / "Deployment/LocalCluster/Scripts/ci-docker-smoke.sh"


class CiVolumeHygieneTests(TestCase):
    """CI test containers on the shared runner must not leave anonymous Docker volumes behind.

    postgres:18 declares /var/lib/postgresql and redis declares /data as volumes. A forced removal keeps
    anonymous volumes, which once left about one 50 MB PostgreSQL volume per CI run on the node.
    """

    def run_arguments(self, service_name, image_variable):
        source = CI_SMOKE.read_text(encoding="utf-8")
        start = source.find(f'docker run -d --name "${service_name}"')
        if start < 0:
            self.fail(f"no docker run for {service_name} in {CI_SMOKE.name}")
        end = source.find(image_variable, start)
        if end < 0:
            self.fail(f"no image reference {image_variable} after docker run for {service_name}")
        return source[start:end]

    def test_ci_smoke_postgres_keeps_its_data_on_tmpfs(self):
        self.assertIn("--tmpfs /var/lib/postgresql:rw,size=1073741824", self.run_arguments("postgres", "$POSTGRES_IMAGE"))

    def test_ci_smoke_redis_keeps_its_data_on_tmpfs(self):
        self.assertIn("--tmpfs /data:rw,size=67108864", self.run_arguments("redis", "$REDIS_IMAGE"))

    def test_ci_smoke_cleanup_is_scoped_to_labelled_run_resources(self):
        source = CI_SMOKE.read_text(encoding="utf-8")
        self.assertIn("--label localcluster.ci.owner=ci-smoke", source)
        self.assertIn('docker rm -f "$container"', source)
        self.assertIn('docker network rm "$network"', source)
