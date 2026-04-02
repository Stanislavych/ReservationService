using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ReservationService.Application.Base
{
    public abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated");
            return int.Parse(userIdClaim);
        }

        protected string GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
                return "Customer";

            return roleClaim;
        }
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
