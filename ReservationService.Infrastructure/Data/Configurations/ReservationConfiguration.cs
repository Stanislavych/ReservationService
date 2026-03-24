using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Reservations;
using ReservationService.Domain.Reservations.ValueObjects;
using ReservationService.Domain.Tables;
using ReservationService.Domain.Users;

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

            entity.Property(r => r.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            entity.Property(r => r.TableId)
                .IsRequired()
                .HasColumnName("table_id");

            entity.Property(r => r.Version)
                .IsRequired()
                .HasDefaultValue(1);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Table>()
                .WithMany()
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property<DateTime>("start_time");
            entity.HasIndex("TableId", "start_time")
                .IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_Reservation_EndTime", "end_time > start_time"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Reservation_GuestsCount", "guests_count > 0"));
            entity.ToTable(t => t.HasCheckConstraint("FK_Reservations_User", "user_id > 0"));
            entity.ToTable(t => t.HasCheckConstraint("FK_Reservations_Table", "table_id > 0"));
        }
    }
}
