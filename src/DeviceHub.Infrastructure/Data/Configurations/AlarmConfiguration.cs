using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
    {
        public void Configure(EntityTypeBuilder<Alarm> builder)
        {
            builder.ToTable("Alarms");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Message)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.AcknowledgedBy)
                .HasMaxLength(50);

            builder.Property(a => a.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(a => a.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(a => a.ResolvedAt);

            // 索引：按设备 + 时间查询
            builder.HasIndex(a => new { a.DeviceId, a.CreatedAt });

            // 索引：查未确认的
            builder.HasIndex(a => a.Status);

            // 外键：关联设备
            builder.HasOne(a => a.Device)
                .WithMany()
                .HasForeignKey(a => a.DeviceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
