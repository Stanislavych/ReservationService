using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.Commands.Reservation;
using ReservationService.Application.DTOs;

namespace ReservationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody]CreateReservationCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
