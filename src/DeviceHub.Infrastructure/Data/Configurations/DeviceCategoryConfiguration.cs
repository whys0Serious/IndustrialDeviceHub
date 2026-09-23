using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data.Configurations
{
    /// <summary>
    /// DeviceCategory实体的数据库映射配置
    /// </summary>
    public class DeviceCategoryConfiguration : IEntityTypeConfiguration<DeviceCategory>
    {
        public void Configure(EntityTypeBuilder<DeviceCategory> builder)
        {
            //表名
            builder.ToTable("DeviceCategories");

            //主键
            builder.HasKey(x => x.Id);

            //Name：必填 最长50
            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            //Description：可空 最长200
            builder.Property(x => x.Description)
                   .HasMaxLength(200)
                   .IsRequired(false);
        }
    }
}
