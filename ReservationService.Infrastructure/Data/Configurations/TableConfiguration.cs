using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Tables;
using ReservationService.Domain.Tables.ValueObjects;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> entity)
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Number)
                .HasConversion(
                number=>number.Value,
                value=>TableNumber.Create(value)
                )
                .IsRequired();

            entity.Property(t => t.Type)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(t => t.Zone)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(t => t.Capacity)
                .HasConversion(
                capacity => capacity.Value,
                value => Capacity.Create(value)
                )
                .IsRequired();
        }
    }
}
