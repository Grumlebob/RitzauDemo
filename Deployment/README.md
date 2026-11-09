# Deployment

RitzauDemo supports one deployment target: [LocalSingleNode](LocalSingleNode/HowToDeployLocalSingleNode.md). It runs an isolated app, PostgreSQL, and Redis stack on a native Linux host and serves the demo over the LAN.

The [release contract](Common/ReleaseContract.md) and `Deployment/Common` scripts handle immutable image and migration provenance, shared runner admission, and scoped CI cleanup. [LocalSingleNode operations](LocalSingleNode/Operations.md) cover setup, deployment, health, backups, recovery, and maintenance.

Public hostname support is documented separately in [Public setup](LocalSingleNode/PublicSetup.md). It remains disabled until the hostname and public exposure are explicitly approved.

Host services, the deployment lock, and Docker are shared with other applications. Use the supported ownership checks and scoped cleanup scripts. Never prune shared Docker resources or stop another application's services.
