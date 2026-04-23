using MediatR;
using Microsoft.Extensions.Logging;
using ReservationService.Application.Commands;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReservationService.Application.Behaviors
{
    public class IdempotentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IIdempotentRequest<TResponse>
    {
        private readonly IIdempotencyRepository _repository;
        private readonly ILogger<IdempotentBehavior<TRequest, TResponse>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public IdempotentBehavior(IIdempotencyRepository repository, ILogger<IdempotentBehavior<TRequest, TResponse>> logger, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var key = request.IdempotencyKey;
            var commandType = typeof(TRequest).Name;
            var existingRecord = await _repository.GetByKeyAsync(key,commandType,cancellationToken);

            if (existingRecord != null)
            {
                _logger.LogInformation("Idempotent request detected. Returning cached response for key: {Key}, Command: {CommandType}",
                    key,commandType);

                if (existingRecord.IsSuccessful)
                {
                    return JsonSerializer.Deserialize<TResponse>(existingRecord.Response);
                }

                throw new Exception(existingRecord.ErrorMessage);
            }

            TResponse response = default;
            int statusCode = 200;
            bool isSuccessful = true;
            string errorMessage = null;

            try
            {
                response = await next();

                return response;
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                statusCode = 400;
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                var record = new IdempotencyRecord(
                    key,
                    commandType,
                    JsonSerializer.Serialize(response),
                    statusCode,
                    isSuccessful,
                    errorMessage
                    );

                await _repository.AddAsync( record,cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
