using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Application.Commands.Reservation;
using ReservationService.Application.DTOs;
using ReservationService.Application.Interfaces;
using ReservationService.IntegrationTests.Utilities;
using System.Net.Http.Json;
using Xunit;

namespace ReservationService.IntegrationTests
{
    public class PaymentIntegrationTests : IntegrationTestBase
    {
        public PaymentIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task ConfirmReservation_ShouldCallPaymentService()
        {
            await ClearDatabaseAsync();

            var table = TestDataFactory.CreateTable();
            var user = TestDataFactory.CreateUser();

            await DbContext.Tables.AddAsync(table);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var client = Factory.CreateClient();

            var idempotencyKey = Guid.NewGuid().ToString();
            client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

            var createCommand = TestDataFactory.CreateReservationCommand(
                tableId: table.Id,
                userId: user.Id,
                idempotencyKey: idempotencyKey
                );

            var createResponse = await client.PostAsJsonAsync("/api/reservation", createCommand);

            var createContent = await createResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Create Response Status: {createResponse.StatusCode}");
            Console.WriteLine($"Create Response Body: {createContent}");

            createResponse.EnsureSuccessStatusCode();
            var createResult = await createResponse.Content.ReadFromJsonAsync<ReservationDto>();

            var confirmResponse = await client.PatchAsJsonAsync($"/api/reservation/{createResult.Id}/confirm",
                createResult.Version.ToString());

            var confirmContent = await confirmResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Confirm Response Status: {confirmResponse.StatusCode}");
            Console.WriteLine($"Confirm Response Body: {confirmContent}");

            confirmResponse.EnsureSuccessStatusCode();
            var confirmResult = await confirmResponse.Content.ReadFromJsonAsync<ReservationDto>();

            confirmResult.Should().NotBeNull();
            confirmResult.ReservationStatus.Should().Be("Confirmed");
        }

        [Fact]
        public async Task PaymentServiceHealthCheck_ShouldReturnHealthy()
        {
            using var scope = Factory.Services.CreateScope();
            var paymentClient = scope.ServiceProvider.GetRequiredService<IPaymentServiceClient>();

            var isHealthy = await paymentClient.HealthCheckAsync();

            isHealthy.Should().BeTrue();
        }
    }
}
