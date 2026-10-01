using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    public class WorkOrderLogConfiguration : IEntityTypeConfiguration<WorkOrderLog>
    {
        public void Configure(EntityTypeBuilder<WorkOrderLog> builder)
        {
            builder.ToTable("WorkOrderLogs");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Action).HasMaxLength(100).IsRequired();
            builder.Property(l => l.Remark).HasMaxLength(1000);
            builder.Property(l => l.Operator).HasMaxLength(50);

            builder.HasIndex(l => new { l.WorkOrderId, l.CreatedAt });
        }
    }
}
