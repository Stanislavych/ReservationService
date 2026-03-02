using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Entities;
using ReservationService.Domain.ValueObjects;

namespace ReservationService.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(u => u.PhoneNumber)
                .HasConversion(
                phone => phone.Number,
                value => PhoneNumber.Create(value)
                )
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(u => u.PhoneNumber)
                .IsUnique();
        }
    }
}
