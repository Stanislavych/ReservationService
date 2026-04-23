using FluentAssertions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Tests.Unit.Domain.Reservation.Common;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationConfirmTests : ReservationTestsBase
    {
        [Fact]
        public void Confirm_WhenStatusIsPendingPayment_SetsStatusToConfirmed()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act
            reservation.Confirm();

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [Fact]
        public void Confirm_WhenStatusIsConfirmed_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm(); // First confirm

            // Act
            var act = () => reservation.Confirm();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Can only confirm pending payment reservations");
        }

        [Fact]
        public void Confirm_WhenStatusIsCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Cancel();

            // Act
            var act = () => reservation.Confirm();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Can only confirm pending payment reservations");
        }

        [Fact]
        public void Confirm_WhenStatusIsCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();
            reservation.Complete();

            // Act
            var act = () => reservation.Confirm();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Can only confirm pending payment reservations");
        }
    }
}
