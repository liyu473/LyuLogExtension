# LyuLogExtension

[![NuGet](https://img.shields.io/nuget/v/LyuLogExtension.svg)](https://www.nuget.org/packages/LyuLogExtension/)
[![GitHub](https://img.shields.io/github/license/liyu473/LyuLogExtension)](https://github.com/liyu473/LyuLogExtension)

基于 ZLogger 高性能的日志简易扩展库，内置简单配置的日志记录功能，支持工厂模式和依赖注入两种使用方式。

因为这是简易的日志拓展，目的是为了简化配置，所以除了过滤器配置外，其余使用默认配置足够了，如果对日志配置需求较特殊，那就失去了此拓展的意义。


## 特性

- 📝 **自动日志分级**：Trace/Debug 和 Info 及以上级别分别输出到不同文件
- 🔄 **滚动日志**：按小时自动滚动（可配置），单文件最大 2MB（可配置）
- 📍 **调用位置追踪**：自动记录类名和行号（使用 ZLog* 方法）
- ⚡ **高性能**：基于 ZLogger 的高性能日志框架
- ⚙️ **灵活配置**：支持 appsettings.json 或代码配置，可自定义日志级别和过滤规则

## 依赖项

依赖Zlogger
感谢Zlogger研发团队 ： https://github.com/Cysharp/ZLogger

## 快速开始

### 方式一：从配置文件读取（推荐）

#### 配置文件 + 控制台输出（开发环境推荐）

```csharp
services.AddZLogger(context.Configuration, logging =>
{
    // logging.AddZLoggerConsole();  // 添加控制台输出
});

//直接使用默认
services.AddZLogger(logging =>
{
    logging.AddZLoggerConsole();  // 添加控制台输出
});
```

#### 完整示例

```csharp
return Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
            optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // 根据环境选择配置
        if (context.HostingEnvironment.IsDevelopment())
        {
            // 开发环境：配置文件 + 控制台
            services.AddZLogger(context.Configuration, logging =>
            {
                logging.AddZLoggerConsole();
            });
        }
        else
        {
            // 生产环境：仅配置文件
            services.AddZLogger(context.Configuration);
        }
        
        // 其他服务注册...
    });
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

## 使用方式（前提是配置好了）

### 方式一：工厂模式（静态使用）

适用于不使用依赖注入的场景，如控制台应用、类库等。

#### 基本用法

```csharp
using LogExtension;

// 获取日志记录器
var logger = ZlogFactory.Get<Program>();

// 记录日志
logger.ZLogInformation("应用启动");
logger.ZLogDebug($"调试信息: {Value}");
```

### 方式二：依赖注入（DI）

适用于 ASP.NET Core、Worker Service 等支持依赖注入的场景。

#### 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册日志服务：

```csharp
using LogExtension;

var builder = WebApplication.CreateBuilder(args);

// 添加 ZLogger 日志服务（使用默认配置）
builder.Services.AddZLogger();

var app = builder.Build();
```

#### 在类中注入使用

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

#### 在控制器中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(ILogger<WeatherController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.ZLogInformation($"获取天气数据");
        return Ok(new { Temperature = 25 });
    }
}
```

#### 
### 常见日志类别名称

| 类别名称 | 说明 |
|---------|------|
| `System.Net.Http.HttpClient` | HttpClient 的所有日志 |
| `System.Net.Http.HttpClient.{name}` | 指定名称的 HttpClient |
| `Microsoft.EntityFrameworkCore` | EF Core 所有日志 |
| `Microsoft.EntityFrameworkCore.Database.Command` | EF Core SQL 命令日志 |
| `Microsoft.AspNetCore` | ASP.NET Core 框架日志 |
| `Microsoft.Hosting.Lifetime` | 应用程序生命周期日志 |

## 日志输出

### 默认配置

- **Trace/Debug 日志**：输出到 `logs/trace/` 目录（可通过 `TraceLogPath` 配置）
- **Info 及以上日志**：输出到 `logs/` 目录（可通过 `InfoLogPath` 配置）
- **滚动策略**：每小时自动滚动（可通过 `RollingInterval` 配置），单文件超过 2MB 时自动创建新文件（可通过 `RollingSizeKB` 配置）
- **文件名格式**：`yyyy-MM-dd-HH_001.log`

### 日志格式

```
2024-11-15 14:30:25.123 [INF] [MyNamespace.MyClass:42] 这是日志消息
2024-11-15 14:30:26.456 [ERR] [MyNamespace.MyClass:45] 发生错误
异常: System.InvalidOperationException: 操作无效
堆栈: at MyNamespace.MyClass.Method() in C:\Path\To\File.cs:line 45
```

格式说明：
- 时间戳（本地时间）

- 日志级别（3 字符缩写：TRC/DBG/INF/WRN/ERR/CRT）

- 类名和行号

- 日志消息

- 异常信息（如果有）

  

### ⚠️ 注意事项

1. **必须使用字符串插值**：ZLogger 方法要求使用 `$""` 字符串插值，出于性能角度
   ```csharp
   // ✅ 正确
   logger.ZLogInformation($"消息内容");
   
   // ❌ 错误：会报 CS9205 错误
   logger.ZLogInformation("消息内容");
   ```

2. **异常记录**：第一个参数传递异常对象
   ```csharp
   try {
       // 业务代码
   } catch (Exception ex) {
       logger.ZLogError(ex, $"操作失败: {operation}");
   }
   ```

3. **无参数日志**：即使没有变量，也要使用 `$""`
   ```csharp
   logger.ZLogInformation($"应用启动成功");
   ```

### 如何完全禁用某个类别的日志？

设置日志级别为 `None`：
```json
{
  "ZLogger": {
    "LogLevel": {
      "System.Net.Http.HttpClient": "None"
    }
  }
}
```

或者通过代码：
```csharp
config.CategoryFilters["System.Net.Http.HttpClient"] = LogLevel.None;
```

## License

MIT License

## 相关链接

- [ZLogger 官方文档](https://github.com/Cysharp/ZLogger)
- [Microsoft.Extensions.Logging 文档](https://docs.microsoft.com/aspnet/core/fundamentals/logging)
