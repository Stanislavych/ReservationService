using FluentAssertions;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationTests
    {
        private readonly GuestsCount _validGuestsCount;
        private readonly TimeRange _validTimeRange;
        private readonly DateTime _createdAt;
        private readonly string _validName = "John Doe";
        private readonly int _validTableId = 1;
        private readonly int _validUserId = 1;

        public ReservationTests()
        {
            _validGuestsCount = GuestsCount.Create(2);
            _validTimeRange = TimeRange.Create(
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(3));
            _createdAt = DateTime.UtcNow;
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesReservation()
        {
            var reservation = new ReservationService.Domain.Reservations.Reservation(
                _validName,
                _validGuestsCount,
                _validTimeRange,
                string.Empty,
                _validTableId,
                _validUserId,
                _createdAt);

            reservation.Should().NotBeNull();
            reservation.Name.Should().Be(_validName);
            reservation.GuestsCount.Should().Be(_validGuestsCount);
            reservation.ReservationTime.Should().Be(_validTimeRange);
            reservation.TableId.Should().Be(_validTableId);
            reservation.UserId.Should().Be(_validUserId);
            reservation.Status.Should().Be(ReservationStatus.PendingPayment);
            reservation.CreatedAt.Should().Be(_createdAt);
            reservation.Version.Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_WithInvalidName_ThrowsDomainException(string invalidName)
        {
            var act = () => new ReservationService.Domain.Reservations.Reservation(
                invalidName,
                _validGuestsCount,
                _validTimeRange,
                string.Empty,
                _validTableId,
                _validUserId,
                _createdAt);

            act.Should().Throw<DomainException>()
                .WithMessage("Name is required");
        }

        [Fact]
        public void Constructor_WithNullGuestsCount_ThrowsDomainException()
        {
            var act = () => new ReservationService.Domain.Reservations.Reservation(
                _validName,
                null!,
                _validTimeRange,
                string.Empty,
                _validTableId,
                _validUserId,
                _createdAt);

            act.Should().Throw<DomainException>()
                .WithMessage("GuestsCount is required");
        }

        [Fact]
        public void Constructor_WithNullTimeRange_ThrowsDomainException()
        {
            var act = () => new ReservationService.Domain.Reservations.Reservation(
                _validName,
                _validGuestsCount,
                null!,
                string.Empty,
                _validTableId,
                _validUserId,
                _createdAt);

            act.Should().Throw<DomainException>()
                .WithMessage("TimeRange is required");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidTableId_ThrowsDomainException(int invalidTableId)
        {
            var act = () => new ReservationService.Domain.Reservations.Reservation(
                _validName,
                _validGuestsCount,
                _validTimeRange,
                string.Empty,
                invalidTableId,
                _validUserId,
                _createdAt);

            act.Should().Throw<DomainException>()
                .WithMessage("TableId is required");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidUserId_ThrowsDomainException(int invalidUserId)
        {
            var act = () => new ReservationService.Domain.Reservations.Reservation(
                _validName,
                _validGuestsCount,
                _validTimeRange,
                string.Empty,
                _validTableId,
                invalidUserId,
                _createdAt);

            act.Should().Throw<DomainException>()
                .WithMessage("UserId is required");
        }
    }
}
