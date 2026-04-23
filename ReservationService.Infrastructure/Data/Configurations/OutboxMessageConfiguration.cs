using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Common;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> entity)
        {
            entity.ToTable("OutboxMessages");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Payload)
                .IsRequired()
                .HasColumnType("jsonb");

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.IsPublished)
                .IsRequired();

            entity.Property(x => x.RetryCount)
                .IsRequired();

            entity.Property(x => x.Error)
                .IsRequired(false);

            entity.HasIndex(x => new { x.IsPublished, x.CreatedAt })
                .HasDatabaseName("IX_OutboxMessages_Unpublished");
        }
    }
}
