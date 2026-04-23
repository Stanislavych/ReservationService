using FluentAssertions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Tests.Unit.Domain.Reservation.Common;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationCancelTests : ReservationTestsBase
    {
        [Fact]
        public void Cancel_WhenStatusIsPendingPayment_SetsStatusToCancelled()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act
            reservation.Cancel();

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenStatusIsConfirmed_SetsStatusToCancelled()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();

            // Act
            reservation.Cancel();

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenStatusIsAlreadyCancelled_DoesNothing()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Cancel();

            // Act
            reservation.Cancel();

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenStatusIsCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();
            reservation.Complete();

            // Act
            var act = () => reservation.Cancel();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot cancel completed reservation");
        }
    }
}
