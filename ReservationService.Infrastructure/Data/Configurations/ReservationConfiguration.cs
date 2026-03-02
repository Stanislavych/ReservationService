using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Entities;
using ReservationService.Domain.ValueObjects;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> entity)
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(r => r.GuestsCount)
                .HasConversion(
                count => count.Value,
                value => GuestsCount.Create(value)
                )
                .IsRequired()
                .HasColumnName("guests_count");

            entity.OwnsOne(r => r.ReservationTime, rt =>
            {
                rt.Property(t => t.Start)
                .HasColumnName("start_time")
                .IsRequired();

                rt.Property(t => t.End)
                .HasColumnName("end_time")
                .IsRequired();
            });

            entity.Property(r => r.Wish)
                .HasMaxLength(500);

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasColumnName("status");

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);


            entity.Property<DateTime>("start_time");
            entity.HasIndex("TableId", "start_time");

            entity.ToTable(t => t.HasCheckConstraint("CK_Reservation_EndTime", "end_time > start_time"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Reservation_GuestsCount", "guests_count > 0"));
        }
    }
}
