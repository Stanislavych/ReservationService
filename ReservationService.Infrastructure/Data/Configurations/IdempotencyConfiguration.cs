using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Common;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class IdempotencyConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.CommandType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Response)
            .HasColumnType("jsonb");

            builder.Property(x => x.StatusCode)
                .IsRequired();

            builder.Property(x => x.IsSuccessful)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt);

            builder.HasIndex(x => new { x.Key, x.CommandType })
            .IsUnique()
            .HasDatabaseName("IX_IdempotencyRecords_Key_CommandType");

            builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_IdempotencyRecords_CreatedAt");
        }
    }
}
