using FluentAssertions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Tests.Unit.Domain.Reservation.Common;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationLifecycleTests : ReservationTestsBase
    {
        [Fact]
        public void FullLifecycle_PendingPaymentToConfirmedToCompleted()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act & Assert
            reservation.Status.Should().Be(ReservationStatus.PendingPayment);

            reservation.Confirm();
            reservation.Status.Should().Be(ReservationStatus.Confirmed);

            reservation.Complete();
            reservation.Status.Should().Be(ReservationStatus.Completed);
        }

        [Fact]
        public void FullLifecycle_PendingPaymentToCancelled()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act & Assert
            reservation.Status.Should().Be(ReservationStatus.PendingPayment);

            reservation.Cancel();
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
        }

        [Fact]
        public void FullLifecycle_PendingPaymentToConfirmedToCancelled()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act & Assert
            reservation.Status.Should().Be(ReservationStatus.PendingPayment);

            reservation.Confirm();
            reservation.Status.Should().Be(ReservationStatus.Confirmed);

            reservation.Cancel();
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
        }
    }
}
