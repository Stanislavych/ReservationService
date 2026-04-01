using MassTransit.Saga;
using ReservationService.Application.Commands.Reservation;
using ReservationService.Domain.Reservations.ValueObjects;
using ReservationService.Domain.Tables;
using ReservationService.Domain.Tables.Enums;
using ReservationService.Domain.Tables.ValueObjects;
using ReservationService.Domain.Users;
using ReservationService.Domain.Users.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace ReservationService.IntegrationTests.Utilities
{
    public class TestDataFactory
    {
        public static Table CreateTable(int tableNumber = 1, int capacity = 4)
        {
            return Table.Create(
                TableNumber.Create(tableNumber),
                TableType.Standart,
                TableZone.Main,
                Capacity.Create(capacity));
        }

        public static User CreateUser(string firstName = "Test", string lastName = "User", string username = "Test",
            string password = "test", string email = "test")
        {
            return User.Create(
                firstName,
                lastName,
                username,
                email,
                password,
                PhoneNumber.Create("+79161234567"),
                "Customer");
        }

        public static TimeRange CreateTimeRange(DateTime? start = null, int durationHours = 2)
        {
            var startTime = start ?? DateTime.UtcNow.AddHours(1);
            return TimeRange.Create(startTime, startTime.AddHours(durationHours));
        }

        public static CreateReservationCommand CreateReservationCommand(
            int tableId,
            int userId,
            int guestsCount = 2,
            DateTime? startTime = null)
        {
            var start = startTime ?? DateTime.UtcNow.AddHours(1);
            return new CreateReservationCommand(
                Name: "Test Reservation",
                GuestsCount: guestsCount,
                StartTime: start,
                EndTime: start.AddHours(2),
                Wish: "",
                TableId: tableId,
                UserId: userId);
        }
    }
}
