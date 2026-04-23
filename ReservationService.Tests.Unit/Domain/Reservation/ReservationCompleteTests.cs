using FluentAssertions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Tests.Unit.Domain.Reservation.Common;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationCompleteTests : ReservationTestsBase
    {
        [Fact]
        public void Complete_WhenStatusIsConfirmed_SetsStatusToCompleted()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();

            // Act
            reservation.Complete();

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Completed);
        }

        [Fact]
        public void Complete_WhenStatusIsPendingPayment_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();

            // Act
            var act = () => reservation.Complete();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Only confirmed reservation can be completed");
        }

        [Fact]
        public void Complete_WhenStatusIsCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Cancel();

            // Act
            var act = () => reservation.Complete();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Only confirmed reservation can be completed");
        }

        [Fact]
        public void Complete_WhenStatusIsAlreadyCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();
            reservation.Complete();

            // Act
            var act = () => reservation.Complete();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Only confirmed reservation can be completed");
        }
    }
}
