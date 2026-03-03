using ReservationService.Domain.Common;
using ReservationService.Domain.Tables.Enums;
using ReservationService.Domain.Tables.ValueObjects;

namespace ReservationService.Domain.Tables
{
    public class Table : AggregateRoot
    {
        public int Id { get; private set; }
        public TableNumber Number { get;private set; } = null!;
        public TableType Type { get; private set; }
        public TableZone Zone { get; private set; }
        public Capacity Capacity { get; private set; } = null!;

        private Table()
        {
            
        }

        public static Table Create(TableNumber tableNumber, TableType tableType, TableZone tableZone, Capacity capacity)
        {
            return new Table
            {
                Number = tableNumber,
                Type = tableType,
                Zone = tableZone,
                Capacity = capacity
            };
        }
    }
}
