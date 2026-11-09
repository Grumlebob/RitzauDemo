# CRUD integration tests

Books CRUD and seeded-account integration tests use [WebAppFactory](../../RitzauDemo.Test/TestSupport/Integration/WebAppFactory.cs), PostgreSQL and Redis Testcontainers. Docker must be running. Container images and resource limits are defined in [TestContainerImages](../../RitzauDemo.Test/TestSupport/Integration/TestContainerImages.cs). Containers use temporary storage and are disposed by the fixture.

The integration collection runs serially because scenarios share a database. It covers Admin CRUD and cache refresh, User read-only access, anonymous denial, direct handler authorization, and Redis invalidation between two app hosts. The seeded-account collection uses real Identity cookies to verify role claims, User denial, Admin success, and logout. Database state resets between scenarios.

Pure request-validation unit tests do not need Docker. The packaged Blazor render smoke is documented in [Browser testing](Browser.md).
