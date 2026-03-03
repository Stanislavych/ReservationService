using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.DTOs;
using ReservationService.Application.Queries.Table;

namespace ReservationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<TableDto>>> GetAvailableTables(
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan time,
            [FromQuery] int guests,
            [FromQuery] TimeSpan? duration = null)
        {
            var query = new GetAvailableTablesQuery(date, time, guests, duration);
            var availableTables = await _mediator.Send(query);

            if (!availableTables.Any())
                return NotFound("No tables available for selected condition");

            return Ok(availableTables);
        }
    }
}
