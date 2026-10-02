# IndustrialDeviceHub

> 工业设备监控与运维管理平台 —— 一个面向中小型工厂设备管理员的桌面端软件（个人作品集项目）

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-red)](https://www.microsoft.com/sql-server/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📖 项目简介

**IndustrialDeviceHub** 是一个面向中小型工厂设备管理员的监控与运维平台。目标是用一套桌面软件，替代传统的"纸质台账 + Excel + 电话"管理方式。

**核心价值：**

- 📋 **设备台账数字化** —— 设备信息、分类、状态集中管理
- 📈 **实时状态可视化** —— Modbus 数据采集 + 实时曲线 + 报警
- 🎫 **工单流转在线化** —— 故障工单创建、流转、历史查询
- 🧠 **故障经验知识化** —— 基于 Semantic Kernel + RAG 的历史案例检索

> ⚠️ **边界声明**
>
> 本项目**不是**真实产线部署的工业控制系统，**不涉及**真实 PLC、运动控制卡、振镜等硬件交互。
> 硬件通信部分通过 **Modbus TCP 仿真器** 完成全链路验证，通信层做接口抽象，预留真实设备适配能力。

---

## 🛠 技术栈

| 层次 | 技术选型 | 说明 |
|------|----------|------|
| **客户端框架** | WPF (.NET 10) | 工业软件桌面端主流 |
| **MVVM 框架** | CommunityToolkit.Mvvm | 轻量、现代、与 DI 集成好 |
| **UI 组件库** | HandyControl | 国内 WPF 项目常用 |
| **图表控件** | LiveChartsCore (v2) | 实时曲线场景成熟方案 |
| **后端框架** | ASP.NET Core Web API (.NET 10) | 现代化 .NET 后端 |
| **ORM** | Entity Framework Core 10 | Code First + 迁移 |
| **数据库** | SQL Server | 工业软件常用 |
| **通信库** | NModbus | Modbus TCP/RTU 开源实现 |
| **AI 编排** | Semantic Kernel | C# 生态 AI 编排框架 |
| **向量数据库** | Qdrant (Docker) | 轻量、易部署 |
| **容器化** | Docker + Docker Compose | 后端一键启动 |
| **测试** | xUnit + Moq + FluentAssertions | 单元测试 |

---

## 🏗 项目架构

### 分层结构

```
┌─────────────────────────────────────────┐
│  DeviceHub.Client (WPF 桌面客户端)        │
│  DeviceHub.Api    (薄 Api，对外接口)      │  ← 表现层
├─────────────────────────────────────────┤
│  DeviceHub.Core   (领域模型、接口、DTO)   │  ← 领域层
├─────────────────────────────────────────┤
│  DeviceHub.Infrastructure                 │  ← 基础设施层
│  (EF Core、Modbus、AI 服务实现)           │
├─────────────────────────────────────────┤
│  DeviceHub.Controls (WPF 自定义控件库)    │  ← UI 组件
│  DeviceHub.Tests    (xUnit 单元测试)      │  ← 测试
└─────────────────────────────────────────┘
```

### 目录结构

```
IndustrialDeviceHub/
├── src/
│ ├── DeviceHub.Client/ # WPF 客户端（WinExe）
│ │ ├── Assets/ # 资源文件
│ │ ├── Attributes/ # 自动注册标记
│ │ ├── Controls/ # 自定义窗口 + 标题栏
│ │ ├── Converters/ # 值转换器
│ │ ├── Extensions/ # DI扩展
│ │ ├── Models/ # 数据模型
│ │ ├── Services/ # 对话框服务
│ │ ├── Themes/ # 主题资源
│ │ ├── ViewModels/ # MVVM ViewModel
│ │ └── Views/ # XAML视图
│ ├── DeviceHub.Controls/ # 独立WPF控件库
│ ├── DeviceHub.Core/ # 领域模型、接口、DTO
│ │ ├── Common/ # 分页基类
│ │ ├── DTOs/ # 数据传输对象
│ │ ├── Entities/ # 实体
│ │ ├── Enums/ # 枚举
│ │ └── Interfaces/ # 服务接口
│ ├── DeviceHub.Infrastructure/ # 数据访问、通信、AI实现
│ │ ├── Data/ # DbContext+配置
│ │ ├── Extensions/ # DI扩展
│ │ ├── Repositories/ # 仓储实现
│ │ └── Services/ # 业务服务实现
│ ├── DeviceHub.Api/ # ASP.NET Core Web API
│ └── DeviceHub.Tests/ # 单元测试
├── docs/ # 设计文档、部署文档
├── docker/ # Dockerfile、docker-compose.yml
├── .gitignore
├── Directory.Build.props # 统一版本号
├── IndustrialDeviceHub.slnx
└── README.md
```

### 数据流

```
Modbus 仿真器 → Infrastructure（通信层） → Core（业务层）
├──→ Client（WPF 直连，主路径）
└──→ Api（对外接口 + AI 编排）
```

## 📦 功能模块

### 已完成

#### ✅ M1 基础框架（第 2 周）

- 6 个项目结构（Client、Controls、Core、Infrastructure、Api、Tests）
- 实体（Device、DeviceCategory、DeviceStatus）
- EF Core DbContext + 配置类
- 初始迁移
- DI 扩展方法

#### ✅ M2 设备管理（第 3-5 周）

**设备管理：**
- 设备台账：新增、编辑、删除（软删除）
- 列表：DataGrid + 分页 + 状态统计
- 搜索：按名称 / 编号模糊匹配
- 筛选：按状态、分类（选中即触发）

**分类管理：**
- 分类 CRUD（增删改查）
- 有设备时禁止删除

**架构与工程：**
- Repository + Service 分层
- 泛型导航（一个 Command 处理所有页面）
- 自动 DI 注册（View / ViewModel 程序集扫描）
- 通用 DialogService（泛型 `ShowDialog<TVM, TV>`）
- `AppMessageBox`（项目风格确认框）
- `BusinessException` + 全局异常处理
- 软删除编号可复用（过滤唯一索引）

**UI：**
- 自定义窗口基类 `CustomWindow`（标题栏 + WindowChrome）
- 主题资源（浅色 / 深色预留）
- 全局状态栏（版本 + 时间，服务状态预留）

#### 🚧 M3 实时监控（第 6-8 周）

**通信层：**
- `IDeviceCommunication` 接口抽象（预留真实 PLC 适配）
- `ModbusDeviceCommunication` 实现（NModbus）
- 断线自动重连

**数据采集：**
- `ModbusPollingService`（1Hz 后台轮询，`BackgroundService`）
- `RealtimeDataCache`（`ConcurrentDictionary` 线程安全缓存）
- `ModbusOptions` 配置化

**实时监控 UI：**
- 温度：HandyControl `WaveProgressBar` 水波纹球
- 压力 / 转速：LiveChartsCore 实时曲线
- 保留最近 60 秒数据点
- 连接状态 + 最后更新时间

**报警系统：**
- `Alarm` 实体 + `AlarmType` / `AlarmStatus` 枚举
- 阈值判断（温度、压力、转速）
- **状态变化触发**（`AlarmTracker`，符合 ISA-18.2 规范）
- 报警恢复（`ResolvedAt`）
- 报警列表：按状态 / 类型筛选、分页、确认
- 底部状态栏报警横幅（有报警时显示，无报警时隐藏）
- 报警确认后通过 `WeakReferenceMessenger` 通知横幅立即刷新
- 
**多设备支持：**
- `DeviceHub.Simulator` 仿真器（模拟 3 台设备）
- `Device` 加 Modbus 配置（`ModbusSlaveId`、`EnableMonitoring`）
- `ModbusPollingService` 遍历所有启用监控的设备
- 实时监控页设备选择器

#### ✅ M4 工单管理（第 9-10 周）

**工单业务：**
- `WorkOrder` + `WorkOrderLog` 实体
- 工单状态机：`Pending → Processing → Closed`，禁止跳步
- 工单号自动生成（`WO-YYYYMMDD-NNN`）
- 创建、编辑、状态流转（开始处理 / 关闭工单）
- 关闭工单需填写处理结果
- 处理记录时间线

**工单 UI：**
- 工单列表：搜索、按状态 / 优先级 / 设备筛选、分页
- 工单编辑对话框（新增 / 编辑）
- 工单详情页：HandyControl `StepBar` 步骤条 + 故障描述 + 处理记录
- 详情页不限制状态（已关闭工单可查看历史）
- 关闭工单对话框（填写处理结果）
- 详情页关闭后自动刷新列表

#### ✅ M5 自定义控件库（第 11-12 周）

**控件（DeviceHub.Controls）：**
- `CustomWindow`（无系统标题栏的窗口基类）
- `TitleBarControl`（自定义标题栏）
- `AppMessageBox`（项目风格消息框）
- `DeviceStatusIndicator`（设备状态指示灯，带阴影/渐变/高光）
- `AlarmBanner`（报警提示条，底部状态栏）

**主题系统：**
- 深色 / 浅色两套主题
- `ThemeManager` 动态切换
- 所有颜色走 `DynamicResource`，切换即时生效
- 主题切换按钮在标题栏右侧

### 进行中

#### ⏳ M6 薄 Api + AI 知识库（第 13-14 周）

- [ ] 对外只读接口（设备、工单）
- [ ] Semantic Kernel 服务端编排
- [ ] Qdrant 向量检索
- [ ] `POST /api/ai/chat`（故障描述 → 相似案例）
- [ ] 对话日志写回 SQL Server

#### ⏳ M7 部署交付（第 15-16 周）

- [ ] WPF 客户端自包含单文件发布
- [ ] 后端 Docker Compose 编排
- [ ] 部署文档
- [ ] README 完善（截图、架构图、技术决策）

---

## 📊 项目状态

| 里程碑 | 时间 | 状态 |
|--------|------|------|
| **M1 基础框架** | 第 2 周 | ✅ 完成 |
| **M2 设备管理** | 第 5 周 | ✅ 完成 |
| **M3 实时监控** | 第 8 周 | ✅ 完成 |
| **M4 工单管理** | 第 10 周 | ✅ 完成 |
| **M5 控件库** | 第 12 周 | ✅ 完成 |
| **M6 薄 Api + AI** | 第 14 周 | 🚧 进行中 |
| **M7 部署交付** | 第 16 周 | ⏳ 计划中 |

---

## 🖼 界面截图

> 📸 截图将在 M7 完成后补充。

**已完成的界面：**

- **设备列表页**：搜索 + 筛选 + 分页 + 状态统计 + DataGrid
- **设备编辑对话框**：表单校验 + 保存
- **主窗口**：自定义标题栏 + 左侧菜单 + 全局状态栏

---

## 🎯 技术决策

### 为什么用 WPF 而不是 WinForm / MAUI？

- WPF 是工业软件桌面端主流，与过往经验一致
- XAML + MVVM 支持复杂 UI 和主题化
- WinForm 数据绑定弱，MAUI 在桌面端不成熟

### 为什么 WPF 直连 Core 而不是走 Api？

- 这是**单机工业软件**，C/S 分离无必要
- 直连省去 HTTP、DTO、序列化、错误处理的大量联调时间
- **但保留 Api 层**：对外集成（MES/SCADA）和 AI 服务端编排（LLM 密钥不能放客户端）

### 为什么"薄 Api"？

- **只做 4 类接口**：AI 对话、设备只读、工单只读、健康检查
- 加接口前先问"WPF 需要吗？"——不需要才放 Api
- 核心是**克制**，避免时间黑洞

### 为什么用 Repository + Service 双层？

- Repository 只碰实体、只做数据访问
- Service 做业务逻辑、DTO 映射、日志
- 便于单元测试（Mock Repository）
- 符合整洁架构

### 为什么 ViewModel 自动注册，Service 显式注册？

- ViewModel 数量多、生命周期统一（Transient）、命名规则清晰 → 自动扫描
- Service 生命周期不同（Scoped/Singleton 混用）、接口实现分离 → 显式注册更清晰
- **折中：约定清晰的部分自动化，需要精细控制的部分显式化**

### 为什么用软删除？

- 工业软件不做物理删除，可追溯、可恢复
- `IsDeleted` 字段 + EF Core 全局查询过滤器，UI 透明

### 为什么用 `IDesignTimeDbContextFactory`？

- 迁移不依赖启动项目的 DI 配置
- CI/CD 友好，服务器上无 UI 也能跑迁移
- 面试展示"设计时 / 运行时分离"的意识

---

## 🚀 快速开始

> 📝 完整部署文档将在 M7 完成后提供。以下是开发环境搭建步骤。

### 环境要求

- **Windows 10/11**
- **Visual Studio 2022**（或 Rider）
- **.NET 10 SDK**
- **SQL Server Express**（或完整版）

### 数据库准备

1. 安装 SQL Server Express，实例名 `SQLEXPRESS`
2. 用 SSMS 连接 `localhost\SQLEXPRESS`
3. 在 `DeviceHub.Api/appsettings.Development.json` 配连接字符串：

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=DeviceHub;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
4.在 Package Manager Console 执行迁移：
```powershell
Update-Database -Project DeviceHub.Infrastructure -StartupProject DeviceHub.Api
```

### 运行客户端

```powershell
cd src
dotnet run --project DeviceHub.Client
```
### 运行测试

```powershell
dotnet test
```

## 📝 开发日志

### M2 完成（第 5 周）

- ✅ 设备 CRUD（Repository + Service 分层）
- ✅ 分页（HandyControl Pagination）
- ✅ 搜索 + 筛选
- ✅ 状态统计
- ✅ 自定义窗口 + 标题栏
- ✅ 主题资源
- ✅ 全局状态栏
- ✅ 通用 DialogService
- ✅ 单元测试（xUnit + Moq）

### M1 完成（第 2 周）

- ✅ 6 个项目结构
- ✅ 实体（Device、DeviceCategory、DeviceStatus）
- ✅ DeviceHubDbContext + 配置类
- ✅ 初始迁移
- ✅ DI 扩展方法

## 📄 许可证

本项目采用 MIT License。

## 👤 作者

**杨滨菁**

- GitHub: [@whys0Serious](https://github.com/whys0Serious)
- Email: yzz7718625@163.com

## 🙏 致谢

- [HandyControl](https://github.com/HandyOrg/HandyControl) —— WPF UI 组件库
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) —— MVVM 框架
- [LiveChartsCore](https://github.com/beto-rodriguez/LiveCharts2) —— 图表控件
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel) —— AI 编排框架
- [NModbus](https://github.com/NModbus/NModbus) —— Modbus 通信库

## ⭐ Star History

如果这个项目对你有帮助，欢迎给一个 ⭐ Star