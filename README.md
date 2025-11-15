# LogExtension

基于 ZLogger 的日志扩展库，提供简单易用的日志记录功能，支持工厂模式和依赖注入两种使用方式。

## 特性

- 🚀 **双模式支持**：支持工厂模式和依赖注入两种使用方式
- 📝 **自动日志分级**：Trace/Debug 和 Info 及以上级别分别输出到不同文件
- 🔄 **滚动日志**：按小时自动滚动，单文件最大 2MB
- 🧵 **线程安全**：支持多线程并发日志记录
- 📍 **调用位置追踪**：自动记录类名和行号
- ⚡ **高性能**：基于 ZLogger 的高性能日志框架

## 依赖项

本库依赖以下 NuGet 包（会自动安装）：

```xml
<PackageReference Include="ZLogger" Version="2.5.10" />
```

## 使用方式

### 方式一：工厂模式（静态使用）

适用于不使用依赖注入的场景，如控制台应用、类库等。

#### 基本用法

```csharp
using LogExtension;

// 获取日志记录器
var logger = ZlogFactory.Get<Program>();

// 记录日志
logger.LogInformation("应用启动");
logger.LogDebug("调试信息: {Value}", 42);
logger.LogWarning("警告信息");
logger.LogError(exception, "发生错误");
```

#### 自定义工厂

如果需要自定义日志配置，可以设置自己的 LoggerFactory：

```csharp
using Microsoft.Extensions.Logging;
using ZLogger;

// 创建自定义工厂
var customFactory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddZLoggerConsole();
    logging.AddZLoggerFile("logs/custom.log");
});

// 设置全局工厂
ZlogFactory.SetFactory(customFactory);

// 之后所有通过 ZlogFactory.Get<T>() 获取的 logger 都会使用自定义配置
var logger = ZlogFactory.Get<MyClass>();
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

#### 使用自定义配置

```csharp
using LogExtension;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 创建自定义工厂
var customFactory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddZLoggerConsole();
});

// 使用自定义工厂注册
builder.Services.AddZLogger(customFactory);

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
        _logger.LogInformation("开始执行任务");
        _logger.LogDebug("处理数据: {Count}", 100);
        _logger.LogInformation("任务完成");
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
        _logger.LogInformation("获取天气数据");
        return Ok(new { Temperature = 25 });
    }
}
```

## 日志输出

### 默认配置

- **Trace/Debug 日志**：输出到 `logs/trace/` 目录
- **Info 及以上日志**：输出到 `logs/` 目录
- **滚动策略**：每小时自动滚动，单文件超过 2MB 时自动创建新文件
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

## 日志级别

| 级别        | 说明                   | 输出位置            |
|-----------|----------------------|-----------------|
| Trace     | 最详细的跟踪信息             | `logs/trace/`   |
| Debug     | 调试信息                 | `logs/trace/`   |
| Information | 一般信息性消息            | `logs/`         |
| Warning   | 警告信息                 | `logs/`         |
| Error     | 错误信息                 | `logs/`         |
| Critical  | 严重错误                 | `logs/`         |

## 最佳实践

### 1. 选择合适的使用方式

- **工厂模式**：适用于控制台应用、类库、不使用 DI 的项目
- **依赖注入**：适用于 ASP.NET Core、Worker Service、使用 DI 的项目

### 2. 使用结构化日志

```csharp
// 推荐：使用占位符
logger.LogInformation("用户 {UserId} 下载了文件 {FileName}，大小: {FileSize} bytes", 
    userId, fileName, fileSize);

// 不推荐：字符串拼接
logger.LogInformation($"用户 {userId} 下载了文件 {fileName}，大小: {fileSize} bytes");
```

### 3. 记录异常信息

```csharp
try
{
    // 业务逻辑
}
catch (Exception ex)
{
    // 将异常对象传递给日志方法
    logger.LogError(ex, "处理请求失败，用户: {UserId}", userId);
    throw;
}
```

## 多框架支持

本库支持以下目标框架：

- ✅ .NET 8.0
- ✅ .NET 9.0
