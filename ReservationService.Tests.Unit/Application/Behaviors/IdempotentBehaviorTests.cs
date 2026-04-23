using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Application.Behaviors;
using ReservationService.Application.Commands;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using System.Text.Json;

namespace ReservationService.Tests.Unit.Application.Behaviors
{
    public class IdempotentBehaviorTests
    {
        private readonly Mock<IIdempotencyRepository> _repositoryMock;
        private readonly Mock<ILogger<IdempotentBehavior<TestIdempotentCommand, string>>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IdempotentBehavior<TestIdempotentCommand, string> _behavior;

        public IdempotentBehaviorTests()
        {
            _repositoryMock = new Mock<IIdempotencyRepository>();
            _loggerMock = new Mock<ILogger<IdempotentBehavior<TestIdempotentCommand, string>>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _behavior = new IdempotentBehavior<TestIdempotentCommand, string>(
                _repositoryMock.Object, _loggerMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WhenFirstRequest_ShouldExecuteAndSaveRecord()
        {
            // Arrange
            var idempotencyKey = "test-key-123";
            var command = new TestIdempotentCommand { IdempotencyKey = idempotencyKey };
            var expectedResponse = "success";

            _repositoryMock.Setup(x => x.GetByKeyAsync(idempotencyKey, typeof(TestIdempotentCommand).Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdempotencyRecord)null);

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult(expectedResponse);

            // Act
            var result = await _behavior.Handle(command, next, CancellationToken.None);

            // Assert
            result.Should().Be(expectedResponse);

            _repositoryMock.Verify(x => x.AddAsync(It.Is<IdempotencyRecord>(r =>
                r.Key == idempotencyKey &&
                r.CommandType == typeof(TestIdempotentCommand).Name &&
                r.IsSuccessful == true),
                It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSameIdempotencyKeyUsedTwice_ShouldReturnCachedResponse()
        {
            // Arrange
            var idempotencyKey = "duplicate-key-456";
            var command = new TestIdempotentCommand { IdempotencyKey = idempotencyKey };
            var cachedResponse = "cached-result";

            var existingRecord = new IdempotencyRecord(
                key: idempotencyKey,
                commandType: typeof(TestIdempotentCommand).Name,
                response: JsonSerializer.Serialize(cachedResponse),
                statusCode: 200,
                isSuccessful: true
            );

            _repositoryMock.Setup(x => x.GetByKeyAsync(idempotencyKey, typeof(TestIdempotentCommand).Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var nextCalled = false;
            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("new-response");
            };

            // Act
            var result = await _behavior.Handle(command, next, CancellationToken.None);

            // Assert
            result.Should().Be(cachedResponse);
            nextCalled.Should().BeFalse();

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRequestFails_ShouldSaveErrorRecord()
        {
            // Arrange
            var idempotencyKey = "error-key-789";
            var command = new TestIdempotentCommand { IdempotencyKey = idempotencyKey };
            var errorMessage = "Something went wrong";

            _repositoryMock.Setup(x => x.GetByKeyAsync(idempotencyKey, typeof(TestIdempotentCommand).Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdempotencyRecord)null);

            RequestHandlerDelegate<string> next = (ct) => throw new Exception(errorMessage);

            // Act
            Func<Task> act = async () => await _behavior.Handle(command, next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(errorMessage);

            _repositoryMock.Verify(x => x.AddAsync(It.Is<IdempotencyRecord>(r =>
                r.Key == idempotencyKey &&
                r.IsSuccessful == false &&
                r.ErrorMessage == errorMessage &&
                r.StatusCode == 400),
                It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenExistingRecordHasError_ShouldThrowAndNotExecuteNext()
        {
            // Arrange
            var idempotencyKey = "error-key-456";
            var command = new TestIdempotentCommand { IdempotencyKey = idempotencyKey };
            var errorMessage = "Previous error";

            var existingRecord = new IdempotencyRecord(
                key: idempotencyKey,
                commandType: typeof(TestIdempotentCommand).Name,
                response: "",
                statusCode: 400,
                isSuccessful: false,
                errorMessage: errorMessage
            );

            _repositoryMock.Setup(x => x.GetByKeyAsync(idempotencyKey, typeof(TestIdempotentCommand).Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var nextCalled = false;
            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("should-not-be-called");
            };

            // Act
            Func<Task> act = async () => await _behavior.Handle(command, next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(errorMessage);
            nextCalled.Should().BeFalse();
        }
    }

    public record TestIdempotentCommand : IIdempotentRequest<string>
    {
        public required string IdempotencyKey { get; init; }
    }
}