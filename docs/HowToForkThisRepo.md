# Fork This Repository

RitzauDemo is a small Blazor CRUD demo with one deployment target: [LocalSingleNode](../Deployment/LocalSingleNode/HowToDeployLocalSingleNode.md). It runs on a native Linux host and serves the app over the LAN. Start with the [deployment guide](../Deployment/README.md) and [requirements](Requirements.md).

## Prepare the fork

Choose a new repository identity and GHCR image before publishing a release. Update `Deployment/Common/release.yml`, the product defaults, and `.env.example` together. Keep account registration and admin seeding disabled for the demo unless its requirements change.

Before the first code PR, configure the repository's native CI runner by following [AgentSetup](../Deployment/LocalSingleNode/AgentSetup.md). Use its `--ci-runner` path and set the repository runner variables it reports. A clone does not grant access to another repository's runners.

## Change the Books demo

The app uses one frontend, Blazor Auto. Keep `frontend-profile.txt` set to `BlazorAuto`. The Books CRUD feature can remain as the product example or be replaced in small slices using [How to add a feature](HowToAddANewFeature.md). Update the server, Blazor client, focused tests, and documentation together.

## Run and publish

Use the [local gate](Test.md#local-gate), open a pull request, and merge only after the required workflow succeeds on that exact head. Main CI publishes an immutable image and migration bundle. Follow the [release contract](../Deployment/Common/ReleaseContract.md) before deployment.

Public hostname setup is a separate optional extension. Keep it disabled until the exact hostname and public exposure are approved.

## Make a standalone product

When the fork becomes a product of its own, follow [Standalone product](StandaloneProduct.md) for the rename and template-material cleanup. If the clone keeps the template as an `upstream` remote, run `gh repo set-default <owner>/<repo>` first; otherwise `gh` acts on the template.
