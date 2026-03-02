using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(u => u.PhoneNumber)
                .IsUnique();
        }
    }
}
