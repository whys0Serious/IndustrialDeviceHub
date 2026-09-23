using DeviceHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Data
{
    /// <summary>
    /// EF Core 数据库上下文。
    /// 负责：
    /// 1. 暴露 DbSet（Devices、DeviceCategories）
    /// 2. 自动扫描并应用实体配置（IEntityTypeConfiguration）
    /// 3. 作为迁移的入口
    /// </summary>
    public class DeviceHubDbContext : DbContext
    {
        /// <summary>
        /// 构造函数注入DbContextOptions
        /// </summary>
        public DeviceHubDbContext(DbContextOptions<DeviceHubDbContext> options)
            : base(options){}

        /// <summary>
        /// 设备表
        /// </summary>
        public DbSet<Device> Devices => Set<Device>();

        /// <summary>
        /// 设备分类表
        /// </summary>
        public DbSet<DeviceCategory> DeviceCategories => Set<DeviceCategory>();

        /// <summary>
        /// 模型创建时调用
        /// ApplyConfigurationsFromAssembly会自动扫描当前程序集所有实现了IEntityTypeConfiguration的类
        /// 新增实体配置不用改 DbContext
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceHubDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
