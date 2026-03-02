using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Entities;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> entity)
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Number)
                .IsRequired();

            entity.Property(t => t.Type)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(t => t.Zone)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(t => t.Capacity)
                .IsRequired()
                .HasDefaultValue(2);
        }
    }
}
