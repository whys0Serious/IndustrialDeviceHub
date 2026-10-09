using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
            builder.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            builder.Property(u => u.DisplayName).HasMaxLength(50).IsRequired();
            builder.Property(u => u.Role).HasConversion<int>().IsRequired();

            builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_Users_Username");
        }
    }
}
