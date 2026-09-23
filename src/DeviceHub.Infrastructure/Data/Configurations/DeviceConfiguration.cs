using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Device实体的数据库映射配置
    /// </summary>
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.Remark)
                .HasMaxLength(500);

            builder.Property(d => d.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(d => d.Code)
                .IsUnique()
                .HasDatabaseName("IX_Devices_Code");

            builder.HasOne(d => d.Category)
                .WithMany(c => c.Devices)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
