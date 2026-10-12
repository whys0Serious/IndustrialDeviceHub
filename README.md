# IndustrialDeviceHub

> 工业设备监控与运维管理平台 —— 面向中小型工厂设备管理员的桌面端软件（个人作品集项目）

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-red)](https://www.microsoft.com/sql-server/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📖 项目简介

**IndustrialDeviceHub** 是一个面向中小型工厂设备管理员的监控与运维平台。目标是用一套桌面软件，替代传统的“纸质台账 + Excel + 电话”管理方式。

**核心价值：**

- 📋 **设备台账数字化** —— 设备信息、分类、状态集中管理
- 📈 **实时状态可视化** —— Modbus 数据采集 + 实时曲线 + 报警
- 🎫 **工单流转在线化** —— 故障工单创建、流转、历史查询
- 🧠 **故障经验知识化** —— 基于 Semantic Kernel + RAG 的历史案例检索
- 👤 **用户会话管理** —— 登录、切换用户、角色权限（基础 RBAC）

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
| **容器化** | Docker + Docker Compose | 后端一键启动 |
| **测试** | xUnit + Moq + FluentAssertions | 单元测试 |

---

## 🏗 项目架构

### 分层结构

\`\`\`
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
\`\`\`

### 目录结构

\`\`\`
IndustrialDeviceHub/
├── src/
│   ├── DeviceHub.Client/             # WPF 客户端（WinExe）
│   │   ├── Assets/                   # 资源文件
│   │   ├── Attributes/               # 自动注册标记
│   │   ├── Controls/                 # 自定义窗口 + 标题栏
│   │   ├── Converters/               # 值转换器
│   │   ├── Extensions/               # DI 扩展
│   │   ├── Models/                   # 数据模型
│   │   ├── Services/                 # 客户端服务（API 调用、AI 对话、会话）
│   │   ├── Themes/                   # 主题资源
│   │   ├── ViewModels/               # MVVM ViewModel
│   │   └── Views/                    # XAML 视图
│   ├── DeviceHub.Controls/           # 独立 WPF 控件库
│   ├── DeviceHub.Core/               # 领域模型、接口、DTO
│   │   ├── Common/                   # 分页基类
│   │   ├── DTOs/                     # 数据传输对象
│   │   ├── Entities/                 # 实体
│   │   ├── Enums/                    # 枚举
│   │   └── Interfaces/               # 服务接口
│   ├── DeviceHub.Infrastructure/     # 数据访问、通信、AI 实现
│   │   ├── Data/                     # DbContext + 配置
│   │   ├── Extensions/               # DI 扩展
│   │   ├── Repositories/             # 仓储实现
│   │   └── Services/                 # 业务服务实现
│   ├── DeviceHub.Api/                # ASP.NET Core Web API（薄 Api）
│   ├── DeviceHub.Simulator/          # Modbus 仿真器
│   └── DeviceHub.Tests/              # 单元测试
├── docs/                             # 设计文档、部署文档
├── docker/                           # Dockerfile、docker-compose.yml
├── .gitignore
├── Directory.Build.props             # 统一版本号
├── IndustrialDeviceHub.slnx
└── README.md
\`\`\`

### 数据流

\`\`\`
Modbus 仿真器 → Infrastructure（通信层） → Core（业务层）
                                             ├──→ Client（WPF 直连，主路径）
                                             └──→ Api（对外接口 + AI 编排）
\`\`\`

---

## 📦 功能模块

### ✅ M1 基础框架（第 2 周）

- 6 个项目结构（Client、Controls、Core、Infrastructure、Api、Tests）
- 实体（Device、DeviceCategory、DeviceStatus）
- EF Core DbContext + 配置类
- 初始迁移
- DI 扩展方法（`AddCoreServices()` / `AddInfrastructure()`）

### ✅ M2 设备管理（第 3-5 周）

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

### ✅ M3 实时监控（第 6-8 周）

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

**多设备支持：**
- `DeviceHub.Simulator` 仿真器（模拟 3 台设备）
- `Device` 加 Modbus 配置（`ModbusSlaveId`、`EnableMonitoring`）
- `ModbusPollingService` 遍历所有启用监控的设备
- 实时监控页设备选择器

### ✅ M4 工单管理（第 9-10 周）

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

### ✅ M5 自定义控件库（第 11-12 周）

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

### ✅ M6 薄 Api + AI 知识库（第 13-14 周）

**Api（DeviceHub.Api）：**

| 控制器 | 端点 | 说明 |
|--------|------|------|
| `DevicesController` | `GET /api/devices` | 分页 + 关键字搜索 |
| | `GET /api/devices/{id}` | 设备详情 |
| | `GET /api/devices/statistics` | 状态统计聚合 |
| `WorkOrdersController` | `GET /api/workorders` | 分页 + 状态/优先级/设备筛选 |
| | `GET /api/workorders/{id}` | 工单详情 |
| | `GET /api/workorders/{id}/logs` | 处理记录时间线 |
| `AiController` | `POST /api/ai/chat` | 简单对话（不走向量检索） |
| | `POST /api/ai/diagnose` | RAG 故障诊断（检索 + 生成） |
| | `GET /api/ai/test` | LLM 连接测试 |

**架构要点：**
- **薄 Api 原则**：加接口前先问「WPF 需要吗」，需要则 WPF 直连 Core，Api 不重复实现
- DTO 与领域枚举**显式映射**，避免契约泄漏
- `CancellationToken` **全程透传**，支持客户端断开时优雅取消
- `AddCoreServices()` / `AddInfrastructure()` 在 WPF 与 Api **共用**，DI 配置一致
- `/health` 端点覆盖 DB  / Modbus 依赖状态
- AI 对话日志**写回 SQL Server**
- LLM 调用带**超时控制**与**兜底回复**

**AI 编排（Semantic Kernel + 关键词检索）：**
- 用户输入故障描述 → 2-gram 关键词提取 → 仓储关键词匹配历史工单 → 相似度排序取 Top-K → 作为 context 喂给 LLM 生成建议
- 相似度算法：关键词在工单文本（标题 + 描述 + 处理结果）中的命中率
- 检索不到时，prompt 中明示「无匹配案例」，让 LLM 基于工业常识给出兜底建议
- LLM 调用带超时控制（`CancellationTokenSource.CancelAfter`），超时/异常均有兜底回复
- LLM 密钥**仅服务端持有**，客户端不落地

**客户端 AI 助手（AiChatView）：**
- 输入故障描述 → 请求 `/api/ai/diagnose`
- 结果分两块展示：AI 建议 + 相似历史案例（带相似度百分比）
- `IsLoading` / `HasResult` / `HasSimilarCases` 三态控制
- 业务异常与系统异常分离处理

### ✅ 登录与会话管理（M6 后补）

**登录功能：**
- `LoginView` / `LoginViewModel`：用户名密码表单 + 错误提示 + 加载状态
- `CustomWindow` 实现 `IDialogAware` 订阅，支持 ViewModel 主动关窗
- `AuthService`：登录校验 + `EnsureDefaultAdminAsync` 首次启动建默认管理员
- `ISessionService`：当前用户、角色、是否管理员

**应用生命周期：**
- `App.OnStartup`：启动 Host → 初始化默认管理员 → 应用主题 → 弹登录窗 → 登录成功显示主窗口
- `App.OnExit`：同步等待 `Host.StopAsync()`，优雅停止 Modbus 轮询等后台服务
- 关闭主窗口 → 应用退出（默认 `ShutdownMode=OnLastWindowClose`）
- 退出登录 → **主窗口不关**，弹登录窗切换用户；取消则回到主界面

**权限（基础 RBAC）：**
- `UserRole` 枚举：`Admin` / `Operator`
- `MainWindowViewModel.IsAdmin` 控制管理员专属菜单可见性

---

## 📊 项目状态

| 里程碑 | 时间 | 状态 |
|--------|------|------|
| **M1 基础框架** | 第 2 周 | ✅ 完成 |
| **M2 设备管理** | 第 5 周 | ✅ 完成 |
| **M3 实时监控** | 第 8 周 | ✅ 完成 |
| **M4 工单管理** | 第 10 周 | ✅ 完成 |
| **M5 控件库** | 第 12 周 | ✅ 完成 |
| **M6 薄 Api + AI** | 第 14 周 | ✅ 完成 |
| **登录与会话管理** | 额外 | ✅ 完成 |
| **M7 部署交付** | 第 16 周 | 🚧 进行中 |

---

## 🖼 界面截图

> 📸 截图将在 M7 完成后补充。

**已完成的界面：**

- **登录窗**：用户名密码表单 + 错误提示
- **主窗口**：自定义标题栏 + 左侧菜单 + 全局状态栏 + 报警横幅
- **设备列表页**：搜索 + 筛选 + 分页 + 状态统计 + DataGrid
- **实时监控页**：水波纹温度球 + 压力/转速曲线 + 报警列表
- **工单列表页**：多条件筛选 + 步骤条详情
- **AI 助手页**：故障描述输入 + AI 建议 + 相似案例卡片

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

### 为什么“薄 Api”？

- **只做 4 类接口**：AI 对话、设备只读、工单只读、健康检查
- 加接口前先问“WPF 需要吗？”——不需要才放 Api
- 核心是**克制**，避免时间黑洞

### 为什么 `AiController` 拆 `chat` 和 `diagnose` 两个端点？

- 语义不同：`chat` 是通用问答，`diagnose` 是 RAG 检索增强
- 调用场景不同：前者不需要知识库，后者必须走向量检索
- 分开后 AI 编排层职责清晰，也便于后续分别优化

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
- 面试展示“设计时 / 运行时分离”的意识

### 为什么 AI 密钥不放客户端？

- 客户端一旦发布，反编译即可拿到密钥
- 所有 LLM 调用经 Api 服务端编排，客户端只展示结果
- 这是 AI 工程化的基本安全边界

### 为什么第一版用关键词检索而不是向量检索（RAG）？

- **MVP 优先**：向量检索需要额外引入 Embedding 模型、向量库（Qdrant）、数据同步流程，工作量不亚于整个 AI 模块本身
- **关键词检索已经够用**：历史工单数量不大（几百到几千），2-gram 关键词匹配能命中大部分故障描述
- **仍然是「检索 + 生成」模式**：LLM 拿到的不是凭空生成，而是基于真实历史工单的上下文
- **后续可演进**：检索层已经抽象为 `IWorkOrderRepository.SearchByKeywordsAsync`，将来换成向量检索只需替换实现，上层 `AiKnowledgeService` 不用改

---

## 🚀 快速开始

> 📝 完整部署文档将在 M7 完成后提供。以下是开发环境搭建步骤。

### 环境要求

- **Windows 10/11**
- **Visual Studio 2022**（或 Rider）
- **.NET 10 SDK**
- **SQL Server Express**（或完整版）
- **Docker Desktop**

### 数据库准备

1. 安装 SQL Server Express，实例名 `SQLEXPRESS`
2. 用 SSMS 连接 `localhost\SQLEXPRESS`
3. 在 `DeviceHub.Api/appsettings.Development.json` 配连接字符串：

\`\`\`json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=DeviceHub;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
\`\`\`

4. 在 Package Manager Console 执行迁移：

\`\`\`powershell
Update-Database -Project DeviceHub.Infrastructure -StartupProject DeviceHub.Api
\`\`\`

### 启动 Modbus 仿真器

\`\`\`powershell
cd src
dotnet run --project DeviceHub.Simulator
\`\`\`

### 启动后端 Api

\`\`\`powershell
cd src
dotnet run --project DeviceHub.Api
\`\`\`

Swagger 地址：`https://localhost:{port}/swagger`

### 运行客户端

\`\`\`powershell
cd src
dotnet run --project DeviceHub.Client
\`\`\`

默认管理员：`admin / admin123`（首次启动自动创建）

### 运行测试

\`\`\`powershell
dotnet test
\`\`\`

---

## 📝 开发日志

### M6 完成 + 登录功能（第 14 周 + 额外）

- ✅ 薄 Api：设备 / 工单只读接口 + 统计
- ✅ AI 对话与 RAG 诊断（Semantic Kernel + 关键词检索）
- ✅ 健康检查 `/health` 覆盖 DB  / Modbus
- ✅ AI 对话日志写回 SQL Server
- ✅ 登录、退出登录、切换用户
- ✅ `CustomWindow` 实现 `IDialogAware` 订阅
- ✅ 应用生命周期（启动流程 + Host 优雅停止）

### M5 完成（第 12 周）

- ✅ 自定义控件库（CustomWindow、TitleBarControl、AlarmBanner、DeviceStatusIndicator）
- ✅ 深浅色主题系统
- ✅ `AppMessageBox` 项目风格消息框

### M4 完成（第 10 周）

- ✅ 工单状态机（Pending → Processing → Closed）
- ✅ 工单号自动生成
- ✅ 处理记录时间线
- ✅ HandyControl StepBar 详情页

### M3 完成（第 8 周）

- ✅ Modbus 通信层抽象
- ✅ 1Hz 后台轮询 + 线程安全缓存
- ✅ 实时曲线 + 报警系统
- ✅ 多设备支持 + 仿真器

### M2 完成（第 5 周）

- ✅ 设备 CRUD（Repository + Service 分层）
- ✅ 分页、搜索、筛选、状态统计
- ✅ 自定义窗口 + 标题栏
- ✅ 通用 DialogService
- ✅ 单元测试（xUnit + Moq）

### M1 完成（第 2 周）

- ✅ 6 个项目结构
- ✅ 实体（Device、DeviceCategory、DeviceStatus）
- ✅ DeviceHubDbContext + 配置类
- ✅ 初始迁移
- ✅ DI 扩展方法

---

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