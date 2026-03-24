using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Infrastructure.Data;
using Xunit;

namespace ReservationService.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;
        protected readonly IServiceScope Scope;
        protected readonly ApplicationDbContext DbContext;
        protected readonly IMediator Mediator;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            Scope = factory.Services.CreateScope();
            DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        }

        public void Dispose()
        {
            Scope?.Dispose();
            Client?.Dispose();
        }

        protected async Task ClearDatabaseAsync()
        {
            await DbContext.Reservations.ExecuteDeleteAsync();
            await DbContext.Tables.ExecuteDeleteAsync();
            await DbContext.Users.ExecuteDeleteAsync();
        }
    }
}
