namespace ContentHub.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "IntegrationTests";
}