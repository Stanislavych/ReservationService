using ReservationService.Domain.Tables.Enums;
using ReservationService.Domain.Tables.ValueObjects;

namespace ReservationService.Application.DTOs
{
    public record TableDto(TableNumber TableNumber, TableType TableType, TableZone TableZone, Capacity Capacity);
}
