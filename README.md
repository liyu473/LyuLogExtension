# LyuLogExtension

[![NuGet](https://img.shields.io/nuget/v/LyuLogExtension.svg)](https://www.nuget.org/packages/LyuLogExtension/)

基于 ZLogger 的日志扩展库。


## 安装

```bash
dotnet add package LyuLogExtension
```

## 使用方式

### 方式一：依赖注入（ASP.NET Core / Host）

```csharp
using LogExtension.Builder;
using LogExtension.Extensions;
using Microsoft.Extensions.Logging;

services.AddZLogger(builder => builder
    // 文件输出配置
    .AddFileOutput("logs/trace/", minLevel:LogLevel.Trace, maxLevel:LogLevel.Debug)  // Trace + Debug
    .AddFileOutput("logs/info/", LogLevel.Information)              // Info 及以上
    .AddFileOutput("logs/error/", LogLevel.Error)                   // Error 及以上
    .AddFileOutput("logs/debug/", LogLevel.debug, null, RollingInterval.Hour, 2048) //debug使用独立滚动配置
    
    // 控制台输出
    .WithConsole()              // 带时间戳
    // .WithConsoleDetails()    // 带时间戳和类名
    
    // 过滤器
    .FilterMicrosoft()          // 过滤 Microsoft 命名空间 (Warning+)
                    			//.FilterMicrosoft(LogLevel.Information) 
    .FilterSystem()             // 过滤 System 命名空间 (Warning+)
    .WithFilter("MyApp.Verbose", LogLevel.Warning)  // 自定义过滤
    
    // 滚动配置（全局默认）
    .WithRollingInterval(RollingInterval.Day)  // 按天滚动
    .WithRollingSizeKB(4096)                   // 单文件最大 4MB
);
```

### 方式二：静态方式（控制台应用 / 无 DI 场景）

```csharp
using LogExtension;
using LogExtension.Builder;
using Microsoft.Extensions.Logging;

// 配置
ZLogFactory.Configure(builder => builder
    .AddFileOutput("logs/trace/", LogLevel.Trace, LogLevel.Debug)
    .AddFileOutput("logs/info/", LogLevel.Information)
    .WithConsole()
    .FilterMicrosoft()
);

// 获取 Logger
var logger = ZLogFactory.Get<Program>();
logger.ZLogInformation($"应用启动");
```

### 方式三：从配置文件加载

```csharp
services.AddZLogger(configuration, "ZLogger");

// 配置文件 + 链式覆盖
services.AddZLogger(configuration, builder => builder
    .WithConsole()
    .FilterMicrosoft(),
    "ZLogger"
);
```

**appsettings.json 示例：**

```json
{
  "ZLogger": {
    "GlobalRollingInterval": "Day",
    "GlobalRollingSizeKB": 4096,
    "Outputs": [
      {
        "Path": "logs/trace/",
        "MinLevel": "Trace",
        "MaxLevel": "Debug"
      },
      {
        "Path": "logs/info/",
        "MinLevel": "Information"
      },
      {
        "Path": "logs/error/",
        "MinLevel": "Error"
      }
    ],
    "LogLevel": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

## 文件输出配置

### 快捷方法

```csharp
.AddTraceOutput()           // logs/trace/ (Trace + Debug)
.AddTraceOutput("custom/")  // 自定义路径

.AddInfoOutput()            // logs/ (Info+)
.AddInfoOutput("custom/")   // 自定义路径

.AddErrorOutput()           // logs/error/ (Error+)
.AddErrorOutput("custom/")  // 自定义路径
```

## 滚动配置

```csharp
// 全局默认配置（对所有未单独配置的输出生效）
.WithRollingInterval(RollingInterval.Day)   // Hour / Day / Month / Year
.WithRollingSizeKB(4096)                    // 单文件最大大小 KB

// 单个输出独立配置（覆盖全局）
.AddFileOutput("logs/error/", LogLevel.Error, null, RollingInterval.Hour, 2048)
```

**默认值：**
- 滚动间隔：`Hour`（每小时）
- 单文件大小：`2048` KB（2MB）

## 控制台配置

```csharp
.WithConsole()          // 时间戳格式：2025-01-01 12:00:00.000 [INF] 消息
.WithConsoleDetails()   // 详细格式：2025-01-01 12:00:00.000 [INF] [MyApp.Service] 消息
```

## 过滤器配置

### 全局过滤器

对所有输出（文件和控制台）生效。

```csharp
// 快捷方法
.FilterMicrosoft()                          // Microsoft 命名空间 Warning+
.FilterMicrosoft(LogLevel.Error)            // Microsoft 命名空间 Error+
.FilterSystem()                             // System 命名空间 Warning+
.FilterSystem(LogLevel.Error)               // System 命名空间 Error+

// 自定义过滤
.WithFilter("MyApp.Verbose", LogLevel.Warning)
.WithFilter("System.Net.Http", LogLevel.Error)

// 批量过滤
.WithFilters(new Dictionary<string, LogLevel>
{
    ["Microsoft"] = LogLevel.Warning,
    ["System"] = LogLevel.Warning,
    ["MyApp.Debug"] = LogLevel.Information
})
```

### 输出独立过滤器

每个输出（文件/控制台）可以有独立的过滤器。

**文件输出独立过滤器：**

```csharp
services.AddZLogger(builder => builder
    .FilterMicrosoft()  // 全局过滤器
    .FilterSystem()
    
    // Info 输出 - 使用全局过滤器（默认）
    .AddInfoOutput("logs/info/")
    
    // Trace 输出 - 不使用全局过滤器，只用独立过滤器
    .AddFileOutput("logs/trace/", LogLevel.Trace, LogLevel.Debug)
        .WithoutGlobalFilters()//移除全局过滤器配置
        .WithOutputFilter("System.Net.Http", LogLevel.Debug)
    
    // Error 输出 - 使用全局 + 额外独立过滤器
    .AddErrorOutput("logs/error/")
        .WithOutputFilter("MyApp.Verbose", LogLevel.Error)
);
```

**控制台独立过滤器：**

```csharp
services.AddZLogger(builder => builder
    .FilterMicrosoft()  // 全局过滤器
    .AddInfoOutput()
    
    // 控制台使用独立过滤器
    .WithConsole()
        .WithConsoleFilter("Microsoft", LogLevel.Error)
        .WithConsoleWithoutGlobalFilters()
);
```

**过滤器优先级：** 独立过滤器 > 全局过滤器

## 日志记录

```csharp
// 注入方式
public class MyService(ILogger<MyService> logger)
{
    public void DoWork()
    {
        logger.ZLogTrace($"跟踪信息");
        logger.ZLogDebug($"调试信息: {value}");
        logger.ZLogInformation($"普通信息");
        logger.ZLogWarning($"警告信息");
        logger.ZLogError($"错误信息");
        logger.ZLogCritical($"严重错误");
        
        // 带异常
        try { ... }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"操作失败: {operation}");
        }
    }
}

// 静态方式
var logger = ZLogFactory.Get<Program>();
logger.ZLogInformation($"消息");
```

> ⚠️ **注意**：必须使用 `$""` 字符串插值语法，否则会编译报错。

## 日志输出格式

**控制台：**
```
2025-01-01 12:00:00.000 [INF] 应用启动成功
2025-01-01 12:00:00.001 [WRN] 配置缺失
```

**文件：**
```
2025-01-01 12:00:00.000 [INF] [MyApp.Services.UserService:42] 用户登录成功
2025-01-01 12:00:00.001 [ERR] [MyApp.Controllers.ApiController:78] 数据库连接失败
异常: System.InvalidOperationException: Connection timeout
堆栈: at MyApp.Controllers.ApiController.GetData() in ApiController.cs:line 78
```

## 日志清理

自动清理过期日志文件，防止磁盘空间耗尽。

```csharp
services.AddZLogger(builder => builder
    .WithRetentionDays(7)                           // 保留 7 天（默认值）
    .WithCleanupInterval(TimeSpan.FromHours(1))     // 每小时检查一次（默认值）
    // .DisableBackgroundCleanup()                  // 可选：禁用后台清理，仅启动时清理
    .AddInfoOutput()
);

// 关闭自动清理
services.AddZLogger(builder => builder
    .WithRetentionDays(0)  // 0 表示不清理
    .AddInfoOutput()
);
```

**默认行为：**
- 启动时立即清理过期日志
- 后台每小时检查一次
- 保留最近 7 天的日志
- 按文件最后修改时间判断是否过期

## 默认配置

| 配置项 | 默认值 |
|--------|--------|
| 输出路径 | `logs/` |
| 日志级别 | `Trace` 及以上（全部） |
| 滚动间隔 | 每小时 |
| 单文件大小 | 2MB |
| 控制台 | 关闭 |
| 过滤器 | 无 |
| 日志保留天数 | 7 天 |
| 后台清理间隔 | 1 小时 |
## 链式方法一览

| 方法 | 说明 |
|------|------|
| `AddFileOutput(path, min, max?)` | 添加文件输出 |
| `AddFileOutput(path, min, max, interval, size)` | 添加文件输出（带滚动配置） |
| `AddTraceOutput(path?)` | 快捷：Trace + Debug |
| `AddInfoOutput(path?)` | 快捷：Info 及以上 |
| `AddErrorOutput(path?)` | 快捷：Error 及以上 |
| `WithRollingInterval(interval)` | 全局滚动间隔 |
| `WithRollingSizeKB(size)` | 全局单文件大小 |
| `WithConsole()` | 启用控制台（时间戳） |
| `WithConsoleDetails()` | 启用控制台（时间戳+类名） |
| `WithFilter(category, level)` | 添加全局过滤器 |
| `WithFilters(dict)` | 批量添加全局过滤器 |
| `FilterMicrosoft(level?)` | 过滤 Microsoft 命名空间 |
| `FilterSystem(level?)` | 过滤 System 命名空间 |
| `WithOutputFilter(category, level)` | 为最后文件输出添加独立过滤器 |
| `WithOutputFilters(dict)` | 为最后文件输出批量添加独立过滤器 |
| `WithoutGlobalFilters()` | 最后文件输出不使用全局过滤器 |
| `WithConsoleFilter(category, level)` | 为控制台添加独立过滤器 |
| `WithConsoleFilters(dict)` | 为控制台批量添加独立过滤器 |
| `WithConsoleWithoutGlobalFilters()` | 控制台不使用全局过滤器 |
| `WithRetentionDays(days)` | 设置日志保留天数（0 禁用清理） |
| `WithCleanupInterval(interval)` | 设置后台清理间隔 |
| `DisableBackgroundCleanup()` | 禁用后台清理（仅启动时清理） |
| `FromConfiguration(config, section?)` | 从配置文件加载 |
| `WithAdditionalConfiguration(action)` | 额外 ILoggingBuilder 配置 |

## 致谢

本项目基于 [ZLogger](https://github.com/Cysharp/ZLogger) 构建。

## License

MIT
