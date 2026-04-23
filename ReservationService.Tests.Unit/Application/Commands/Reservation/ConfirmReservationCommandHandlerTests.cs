using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ReservationService.Application.DTOs.Payment;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations.ValueObjects;
using System.Security.Claims;

namespace ReservationService.Tests.Unit.Application.Commands.Reservation
{
    public class ConfirmReservationCommandHandlerTests
    {
        private readonly Mock<IReservationUpdateService> _reservationUpdateServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
        private readonly Mock<IReservationRepository> _reservationRepositoryMock;
        private readonly Mock<IPaymentServiceClient> _paymentServiceClientMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly ReservationService.Application.Commands.Reservation.ConfirmReservationCommand.Handler _handler;

        public ConfirmReservationCommandHandlerTests()
        {
            _reservationUpdateServiceMock = new Mock<IReservationUpdateService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _outboxRepositoryMock = new Mock<IOutboxRepository>();
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _handler = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand.Handler(
                _reservationUpdateServiceMock.Object,
                _unitOfWorkMock.Object,
                _outboxRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _reservationRepositoryMock.Object,
                _paymentServiceClientMock.Object);
        }

        private void SetupHttpContext(int userId = 1, string role = "Customer")
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        }

        private ReservationService.Domain.Reservations.Reservation CreateTestReservation(int id = 1, int userId = 1, int tableId = 1,
            string status = "PendingPayment", long version = 1)
        {
            var reservation = new ReservationService.Domain.Reservations.Reservation(
                "Test",
                GuestsCount.Create(2),
                TimeRange.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3)),
                "",
                tableId,
                userId,
                DateTime.UtcNow);

            typeof(ReservationService.Domain.Reservations.Reservation).GetProperty("Id")?.SetValue(reservation, id);
            typeof(ReservationService.Domain.Reservations.Reservation).GetProperty("Version")?.SetValue(reservation, version);

            return reservation;
        }

        [Fact]
        public async Task Handle_WhenReservationExistsAndPaymentSucceeds_ShouldConfirmAndSave()
        {
            // Arrange
            SetupHttpContext();
            var command = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand(1, 1);
            var reservation = CreateTestReservation();

            _reservationRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            _paymentServiceClientMock.Setup(x => x.ConfirmPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResponse { IsSuccessful = true });

            _reservationUpdateServiceMock.Setup(x => x.ConfirmAsync(
                    It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            _reservationUpdateServiceMock.Verify(x => x.ConfirmAsync(
                It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                1,
                It.IsAny<CancellationToken>()), Times.Once);

            _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenReservationNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            SetupHttpContext();
            var command = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand(999, 1);

            _reservationRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReservationService.Domain.Reservations.Reservation)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Reservation 999 not found");

            _reservationUpdateServiceMock.Verify(x => x.ConfirmAsync(
                It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPaymentFails_ShouldThrowDomainExceptionAndNotConfirm()
        {
            // Arrange
            SetupHttpContext();
            var command = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand(1, 1);
            var reservation = CreateTestReservation();

            _reservationRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            _paymentServiceClientMock.Setup(x => x.ConfirmPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResponse { IsSuccessful = false, Message = "Insufficient funds" });

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Payment failed: Insufficient funds");

            _reservationUpdateServiceMock.Verify(x => x.ConfirmAsync(
                It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenGuestTriesToConfirmOtherUsersReservation_ShouldThrowUnauthorizedAccess()
        {
            // Arrange
            SetupHttpContext(userId: 1, role: "Customer");
            var command = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand(1, 1);
            var reservation = CreateTestReservation(id: 1, userId: 2, version: 1);

            typeof(ReservationService.Domain.Reservations.Reservation).GetProperty("UserId")?.SetValue(reservation, 2);

            _reservationRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            _paymentServiceClientMock.Setup(x => x.ConfirmPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResponse { IsSuccessful = true });

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You don't have permission to modify this reservation");

            _reservationUpdateServiceMock.Verify(x => x.ConfirmAsync(
                It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenManagerTriesToConfirmOtherUsersReservation_ShouldSucceed()
        {
            // Arrange
            SetupHttpContext(userId: 1, role: "Manager");
            var command = new ReservationService.Application.Commands.Reservation.ConfirmReservationCommand(1, 1);
            var reservation = CreateTestReservation();

            typeof(ReservationService.Domain.Reservations.Reservation).GetProperty("UserId")?.SetValue(reservation, 2);

            _reservationRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            _paymentServiceClientMock.Setup(x => x.ConfirmPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResponse { IsSuccessful = true });

            _reservationUpdateServiceMock.Setup(x => x.ConfirmAsync(
                    It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            _reservationUpdateServiceMock.Verify(x => x.ConfirmAsync(
                It.IsAny<ReservationService.Domain.Reservations.Reservation>(),
                1,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

}
