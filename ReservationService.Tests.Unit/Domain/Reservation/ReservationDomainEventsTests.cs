using FluentAssertions;
using ReservationService.Domain.Events;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Tests.Unit.Domain.Reservation.Common;

namespace ReservationService.Tests.Unit.Domain.Reservation
{
    public class ReservationDomainEventsTests : ReservationTestsBase
    {
        [Fact]
        public void Constructor_WhenReservationCreated_AddsReservationCreatedEvent()
        {
            // Arrange & Act
            var reservation = CreateValidReservation();

            // Assert
            var events = reservation.DomainEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Should().BeOfType<ReservationCreated>();
        }

        [Fact]
        public void Confirm_WhenCalled_AddsReservationConfirmedEvent()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.ClearDomainEvents();

            // Act
            reservation.Confirm();

            // Assert
            var events = reservation.DomainEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Should().BeOfType<ReservationConfirmed>();

            var confirmedEvent = events[0] as ReservationConfirmed;
            confirmedEvent!.ReservationId.Should().Be(reservation.Id);
            confirmedEvent.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [Fact]
        public void Cancel_WhenCalled_AddsReservationCancelledEvent()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.ClearDomainEvents();

            // Act
            reservation.Cancel();

            // Assert
            var events = reservation.DomainEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Should().BeOfType<ReservationCancelled>();

            var cancelledEvent = events[0] as ReservationCancelled;
            cancelledEvent!.ReservationId.Should().Be(reservation.Id);
            cancelledEvent.Status.Should().Be(ReservationStatus.Cancelled);
        }

        [Fact]
        public void Complete_WhenCalled_AddsReservationCompletedEvent()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();
            reservation.ClearDomainEvents();

            // Act
            reservation.Complete();

            // Assert
            var events = reservation.DomainEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Should().BeOfType<ReservationCompleted>();

            var completedEvent = events[0] as ReservationCompleted;
            completedEvent!.ReservationId.Should().Be(reservation.Id);
            completedEvent.Status.Should().Be(ReservationStatus.Completed);
        }

        [Fact]
        public void MultipleOperations_AddsMultipleEvents()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.ClearDomainEvents();

            // Act
            reservation.Confirm();
            reservation.Complete();

            // Assert
            var events = reservation.DomainEvents.ToList();
            events.Should().HaveCount(2);
            events[0].Should().BeOfType<ReservationConfirmed>();
            events[1].Should().BeOfType<ReservationCompleted>();
        }

        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            // Arrange
            var reservation = CreateValidReservation();
            reservation.Confirm();

            // Act
            reservation.ClearDomainEvents();

            // Assert
            reservation.DomainEvents.Should().BeEmpty();
        }
    }
}
