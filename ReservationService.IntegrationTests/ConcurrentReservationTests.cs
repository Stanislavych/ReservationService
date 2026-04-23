using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Domain.Exceptions;
using ReservationService.IntegrationTests.Utilities;
using System.Collections.Concurrent;
using Xunit;

namespace ReservationService.IntegrationTests
{
    public class ConcurrentReservationTests : IntegrationTestBase
    {
        public ConcurrentReservationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }
        [Fact]
        public async Task CreateReservation_WhenMultipleRequestsSimultaneously_OnlyOneSucceeds()
        {
            await ClearDatabaseAsync();

            var table = TestDataFactory.CreateTable();
            var user = TestDataFactory.CreateUser();

            await DbContext.Tables.AddAsync(table);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var timeRange = TestDataFactory.CreateTimeRange();
            var concurrentRequests = 10;
            var results = new ConcurrentBag<(bool Success, string Error)>();

            var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
            {
                using var scope = Factory.Services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = TestDataFactory.CreateReservationCommand(
                    tableId: table.Id,
                    userId: user.Id,
                    startTime: timeRange.Start);

                try
                {
                    var result = await mediator.Send(command);

                    results.Add((true, null));
                }
                catch (DomainException ex) when (ex.Message.Contains("not available"))
                {
                    results.Add((false, ex.Message));
                }
                catch (Exception ex)
                {
                    results.Add((false, ex.Message));
                }
            });

            await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            successCount.Should().Be(1, "только одно бронирование должно быть успешным");
            failureCount.Should().Be(concurrentRequests - 1, "остальные должны получить ошибку");

            var reservationsCount = await DbContext.Reservations.CountAsync();
            reservationsCount.Should().Be(1);
        }
    }
}
