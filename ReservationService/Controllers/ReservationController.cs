using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.Commands.Reservation;
using ReservationService.Application.DTOs;
using ReservationService.Application.Queries.Reservation;

namespace ReservationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpPatch("{id}/confirm")]
        public async Task<ActionResult<ReservationDto>> Confirm(int id, [FromBody] long version)
        {
            var command = new ConfirmReservationCommand(id,version);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ReservationDto>> Cancel(int id, [FromBody] long version)
        {
            var command = new CancelReservationCommand(id,version);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpGet("{id}/info")]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            var query = new GetReservationCommand(id);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReservationDto>>> GetByUserId(int userId)
        {
            var query = new GetUserReservationsCommand(userId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
