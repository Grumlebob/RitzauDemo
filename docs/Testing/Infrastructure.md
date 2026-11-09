# CRUD integration tests

The Books CRUD integration test uses [WebAppFactory](../../RitzauDemo.Test/TestSupport/Integration/WebAppFactory.cs), PostgreSQL and Redis Testcontainers. Docker must be running. Container images and resource limits are defined in [TestContainerImages](../../RitzauDemo.Test/TestSupport/Integration/TestContainerImages.cs). Containers use temporary storage and are disposed by the fixture.

The integration collection runs serially because it shares a host and database. The test creates one book, lists and reads it, updates it, checks that another user cannot read it, then deletes it. Database state resets before and after the test.

Pure request-validation unit tests do not need Docker. The packaged Blazor render smoke is documented in [Browser testing](Browser.md).
