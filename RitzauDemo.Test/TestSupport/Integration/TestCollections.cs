// Keep the database integration and packaged browser smoke isolated from parallel work.
[assembly: CollectionBehavior(MaxParallelThreads = 2)]

namespace RitzauDemo.Test.TestSupport.Integration;

public static class TestCollectionNames
{
    public const string Integration = "IntegrationTestCollection";
    public const string E2E = "E2ECollection";
}

/// <summary>Tests that share the default <see cref="WebAppFactory"/>.</summary>
[CollectionDefinition(TestCollectionNames.Integration, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<WebAppFactory>
{
}

/// <summary>The single packaged Blazor browser smoke.</summary>
[CollectionDefinition(TestCollectionNames.E2E, DisableParallelization = true)]
public sealed class E2ECollection
{
}
