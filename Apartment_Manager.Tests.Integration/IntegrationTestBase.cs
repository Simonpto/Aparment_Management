using System.Net.Http.Json;

namespace Apartment_Manager.Tests.Integration;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(ApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Factory.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task LoginAsAdminAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/admin/login", new { password = "test-admin-password" });
        response.EnsureSuccessStatusCode();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ApiFactory> { }
