using ReservationService.Domain.Tables.Enums;

namespace ReservationService.Application.DTOs
{
    public record TableDto(int Id, int TableNumber, TableType TableType, TableZone TableZone, int Capacity);
}
