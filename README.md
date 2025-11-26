# LyuLogExtension

[![NuGet](https://img.shields.io/nuget/v/LyuLogExtension.svg)](https://www.nuget.org/packages/LyuLogExtension/)
[![GitHub](https://img.shields.io/github/license/liyu473/LyuLogExtension)](https://github.com/liyu473/LyuLogExtension)

基于 ZLogger 高性能的日志简易扩展库，内置简单配置的日志记录功能，支持工厂模式和依赖注入两种使用方式。

**简化配置，开箱即用** - 专为快速开发设计，提供合理的默认配置。如需复杂定制，建议直接使用 ZLogger 原生 API。

## ✨ 特性

- 📝 **自动日志分级**：Trace/Debug 和 Info+ 级别分别输出到不同文件
- 🔄 **滚动日志**：按小时自动滚动（可配置），单文件最大 2MB（可配置）
- 📍 **调用位置追踪**：自动记录类名和行号（使用 ZLog* 方法）
- ⚡ **高性能**：基于 ZLogger 的高性能日志框架
- ⚙️ **灵活配置**：支持 appsettings.json 或代码配置，可自定义日志级别和过滤规则

## 🙏 致谢

本项目基于 [ZLogger](https://github.com/Cysharp/ZLogger) 构建，感谢 Cysharp 团队的优秀工作！

## 🚀 快速开始

```csharp
// ASP.NET Core / Web API
var builder = WebApplication.CreateBuilder(args);

//默认配置
builder.Services.AddZLogger(config =>
{
    // 可选：配置日志过滤器（推荐屏蔽框架噪音）
    config.CategoryFilters["Microsoft"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.AspNetCore"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.Hosting.Lifetime"] = LogLevel.Information;
    
    // 控制台输出
    config.AdditionalConfiguration = logging =>
    {
        logging.AddZLoggerConsoleWithTimestamp();
    };
});

var app = builder.Build();
```

**appsettings.json 完整配置示例：**

```json
{
  "ZLogger": {
    "MinimumLevel": "Information",
    "TraceMinimumLevel": "Trace",
    "InfoLogPath": "D:/MyApp/logs/",
    "TraceLogPath": "D:/MyApp/logs/debug/",
    "RollingInterval": "Day",
    "RollingSizeKB": 10240,
    "LogLevel": {
      "Default": "Information",
      "System.Net.Http.HttpClient": "Warning",
      "Microsoft.Extensions.Http": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**配置项说明：**

| 配置项 | 类型 | 默认值 | 说明 |
|-------|------|--------|------|
| `MinimumLevel` | `string` | `Information` | Info 日志最低级别 |
| `TraceMinimumLevel` | `string` | `Trace` | Trace 日志最低级别 |
| `InfoLogPath` | `string` | `logs/` | Info 日志路径（可选） |
| `TraceLogPath` | `string` | `logs/trace/` | Trace 日志路径（可选） |
| `RollingInterval` | `string` | `Hour` | 滚动间隔：`Hour`/`Day`/`Month`/`Year`（可选） |
| `RollingSizeKB` | `int` | `2048` | 单文件大小（KB）（可选） |
| `LogLevel` | `object` | - | 类别过滤器 |

### 方式二：代码配置（无需配置文件）

#### 基础配置（仅文件日志）

```csharp
services.AddZLogger(config =>
{
    // 类别过滤器（推荐配置，屏蔽框架日志）
    config.CategoryFilters["System.Net.Http.HttpClient"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.Extensions.Http"] = LogLevel.Warning;
});
```



#### 完整自定义配置

```csharp
services.AddZLogger(logging =>
{
    // 额外的日志提供程序
    logging.AddZLoggerConsole();
    logging.AddDebug();
}, config =>
{
    // 日志级别配置（可选，有默认值）
    config.MinimumLevel = LogLevel.Information;        // logs/ 文件夹接受的最低日志级别（默认：Information）
    config.TraceMinimumLevel = LogLevel.Trace;         // logs/trace/ 文件夹接受的最低日志级别（默认：Trace）
    
    // 类别过滤器
    config.CategoryFilters["System.Net.Http.HttpClient"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.Extensions.Http"] = LogLevel.Warning;
    
    // 高级配置（可选）
    config.InfoLogPath = "D:/MyApp/logs/";             // Info 日志路径（默认：logs/）
    config.TraceLogPath = "D:/MyApp/logs/debug/";      // Trace 日志路径（默认：logs/trace/）
    config.RollingInterval = RollingInterval.Day;      // 滚动间隔（默认：每小时）
    config.RollingSizeKB = 10240;                      // 单文件大小KB（默认：2048 = 2MB）
});
```

### 配置说明

| 配置项 | 默认值 | 说明 |
|-------|--------|------|
| `MinimumLevel` | `Information` | logs/ 文件夹记录的最低日志级别 |
| `TraceMinimumLevel` | `Trace` | logs/trace/ 文件夹记录的最低日志级别 |
| `CategoryFilters` | 空 | 类别过滤器，用于屏蔽特定命名空间的日志 |
| `InfoLogPath` | `logs/` | Info 及以上日志的输出路径 |
| `TraceLogPath` | `logs/trace/` | Trace/Debug 日志的输出路径 |
| `RollingInterval` | `Hour` | 日志文件滚动间隔（Hour/Day/Month等） |
| `RollingSizeKB` | `2048` | 单个日志文件最大大小（KB） |

## 📝 使用方式

### 🏭 方式一：依赖注入（推荐）

适用于 ASP.NET Core、Worker Service 等支持依赖注入的场景：

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.ZLogInformation($"开始执行任务");
        _logger.ZLogDebug($"处理数据: {100}");
        _logger.ZLogInformation($"任务完成");
    }
}
```

### ⚙️ 方式二：工厂模式（静态使用）

适用于控制台应用、类库等不使用依赖注入的场景：

```csharp
using LogExtension;

// 获取日志记录器
var logger = ZlogFactory.Get<Program>();

// 记录日志
logger.ZLogInformation($"应用启动");
logger.ZLogDebug($"调试信息: {42}");
```

## 📋 常见日志过滤配置

### 推荐的框架日志过滤

| 类别名称 | 说明 |
|---------|------|
| `System.Net.Http.HttpClient` | HttpClient 的所有日志 |
| `System.Net.Http.HttpClient.{name}` | 指定名称的 HttpClient |
| `Microsoft.EntityFrameworkCore` | EF Core 所有日志 |
| `Microsoft.EntityFrameworkCore.Database.Command` | EF Core SQL 命令日志 |
| `Microsoft.AspNetCore` | ASP.NET Core 框架日志 |
| `Microsoft.Hosting.Lifetime` | 应用程序生命周期日志 |

## 📁 日志输出说明

### 默认文件结构

```
your-project/
├── logs/                    # Info+ 级别日志
│   └── 2025-11-26-19_001.log
└── logs/trace/              # Trace/Debug 日志  
    └── 2025-11-26-19_001.log
```

### 日志格式

**控制台输出（彩色）：**
```
2025-11-26 19:14:40.692 [INF] Application started successfully
2025-11-26 19:14:40.693 [WRN] Configuration value is missing
2025-11-26 19:14:40.694 [ERR] Operation failed
```

**文件输出（详细）：**
```
2025-11-26 19:14:40.692 [INF] [MyApp.Services.UserService:42] User login successful: admin
2025-11-26 19:14:40.693 [ERR] [MyApp.Controllers.ApiController:78] Database connection failed
异常: System.InvalidOperationException: Connection timeout
堆栈: at MyApp.Controllers.ApiController.GetData() in C:\MyApp\Controllers\ApiController.cs:line 78
```

  

## ⚠️ 重要提醒

### 1. 字符串插值语法（必须）

```csharp
// ✅ 正确 - 必须使用 $"" 语法
logger.ZLogInformation($"用户登录: {username}");
logger.ZLogInformation($"操作完成");  // 即使无变量也要用 $""

// ❌ 错误 - 会编译报错 CS9205
logger.ZLogInformation("用户登录");
```

### 2. 异常记录

```csharp
try 
{
    // 业务代码
} 
catch (Exception ex) 
{
    logger.ZLogError(ex, $"操作失败: {operation}");
}
```

## 🔧 进阶配置

### 控制台输出格式

内置两种控制台格式：

```csharp
// 1. 简洁格式（仅时间戳 + 级别）
logging.AddZLoggerConsoleWithTimestamp();
// 输出：2025-11-26 19:14:40.692 [INF] 应用启动成功

// 2. 详细格式（时间戳 + 级别 + 类名）  
logging.AddZLoggerConsoleWithDetails();
// 输出：2025-11-26 19:14:40.692 [INF] [MyApp.Services.UserService] 用户登录成功

// 3. 原生格式（ZLogger 默认）
logging.AddZLoggerConsole();
```

### 完全禁用某类别日志

```csharp
// 方式1：代码配置
config.CategoryFilters["System.Net.Http.HttpClient"] = LogLevel.None;

// 方式2：appsettings.json
{
  "ZLogger": {
    "LogLevel": {
      "System.Net.Http.HttpClient": "None"
    }
  }
}
```

## 📄 更新日志

### v1.5.0 (2025-11-26)
- ✨ 新增控制台格式化扩展方法
  - `AddZLoggerConsoleWithTimestamp()` - 简洁格式
  - `AddZLoggerConsoleWithDetails()` - 详细格式
- 📝 更新文档和示例

### v1.4.1 (2025-11-26)
- 🐛 修复控制台日志重复输出问题
- ✨ 优化日志分发逻辑

### v1.4.0 (2025-11-26)  
- ✨ 新增单参数 `AddZLogger(Action<ILoggingBuilder>)` 重载
- 📝 改进文档和示例

---

## 📜 License

[MIT License](https://github.com/liyu473/LyuLogExtension/blob/main/LICENSE)

## 🔗 相关链接

- 📖 [ZLogger 官方文档](https://github.com/Cysharp/ZLogger)
- 📚 [Microsoft.Extensions.Logging 文档](https://docs.microsoft.com/aspnet/core/fundamentals/logging)
- 🐛 [问题反馈](https://github.com/liyu473/LyuLogExtension/issues)
- 💬 [功能建议](https://github.com/liyu473/LyuLogExtension/discussions)
