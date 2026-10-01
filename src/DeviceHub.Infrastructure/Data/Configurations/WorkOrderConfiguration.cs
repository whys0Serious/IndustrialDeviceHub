using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNo).HasMaxLength(50).IsRequired();
            builder.Property(o => o.Title).HasMaxLength(200).IsRequired();
            builder.Property(o => o.Description).HasMaxLength(2000).IsRequired();
            builder.Property(o => o.CreatedBy).HasMaxLength(50);
            builder.Property(o => o.AssignedTo).HasMaxLength(50);
            builder.Property(o => o.Resolution).HasMaxLength(2000);

            builder.Property(o => o.Priority).HasConversion<int>().IsRequired();
            builder.Property(o => o.Status).HasConversion<int>().IsRequired();

            //索引
            builder.HasIndex(o => o.OrderNo).IsUnique().HasDatabaseName("IX_WorkOrders_OrderNo");
            builder.HasIndex(o => new { o.DeviceId, o.CreatedAt });
            builder.HasIndex(o => o.Status);

            //关联设备（可选）
            builder.HasOne(o => o.Device)
                .WithMany()
                .HasForeignKey(o => o.DeviceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            //工单 → 处理记录
            builder.HasMany(o => o.Logs)
                .WithOne(l => l.WorkOrder)
                .HasForeignKey(l => l.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
