# 当前代码基线审计

> 审计日期：2026-10-31
> 分支：`feat/theme`
> 提交：`3ce29b18`（`feat(theme): 添加大·隐藏主题选择行`）
> 主机：Windows 10.0.26300 / x64 / .NET SDK 10.0.402

本审计只记录现状，不代表迁移方案或估算变更。所有数字可由文末命令复现。

## 1. 解决方案结构

| 项目 | 目标框架 | 关键属性 | 说明 |
|---|---|---|---|
| `PCL.Core` | `net10.0-windows` | `UseWPF=true` | 核心库，但内部同时包含 WPF UI 层 |
| `PCL.Core.Test` | `net10.0-windows` | `UseWPF=true` | MSTest；在 slnx 中设置为不参与解决方案构建 |
| `Plain Craft Launcher 2` | `net10.0-windows` | `UseWPF=true`, `WinExe` | 现有 WPF 启动器 |
| `PCL.Core.SourceGenerators` | 由 SDK 决定 | Analyzer | 生命周期源生成器 |

当前解决方案没有任何 Avalonia 引用，也没有 `IPathService`、`ISecretStore`、`IProcessService`、`ICompositionRoot` 等目标接口。

## 2. 代码规模

| 范围 | 文件数 | 行数 | 备注 |
|---|---:|---:|---|
| `PCL.Core` 全部 `.cs` | 549 | 46,361 | 不含 `bin`/`obj` |
| `PCL.Core/UI` | 137 | 11,039 | WPF 控件、动画、主题、模糊效果 |
| `PCL.Core` 非 UI 代码 | 412 | 35,322 | 仍含 26 个直接引用 WPF 的文件 |
| `Plain Craft Launcher 2` 的 `.cs` + `.xaml` | 681 | 172,312 | 现有 WPF 启动器主体 |
| 测试方法 / 测试类 | 188 / 48 | — | MSTest |

## 3. WPF 耦合面

- `PCL.Core` 中有 **67** 个 `.cs` 文件直接引用 `System.Windows`、`Microsoft.Win32` 或 `System.Drawing`。
- 其中 **26** 个在 `UI/` 目录之外，说明 WPF 依赖不只是在 UI 层：
  - `App/Basics.cs`、`App/Essentials/ApplicationService.cs`、`App/Essentials/MainWindowService.cs`、`App/Essentials/RpcService.cs`、`App/Essentials/StartupValidation.cs`
  - `App/Localization/*`：语言资源是 XAML，字体服务直接使用 WPF
  - `IO/Files.cs`、`IO/Net/Http/HttpProxyManager.cs`、`Logging/LogService.cs`
  - `Minecraft/Java/Scanner/RegistryJavaScanner.cs`
  - `Utils/WpfUtils.cs`、`Utils/OS/ClipboardUtils.cs`、`Utils/OS/DragHelper.cs`、`Utils/OS/SystemTheme.cs`
- `PCL.Core` 还有 8 个 XAML 文件；`Plain Craft Launcher 2` 有 94 个 XAML 文件。
- `PCL.Core` 的 `App/Localization/Languages/zh-CN.xaml` 等语言资源约 36 万字节/语言，UI 本地化与 WPF 资源字典绑定较深。

**结论：** “Core 去 WPF”不能只改项目属性。需要先把 `PCL.Core/UI` 与 WPF 专属实现拆出，再为日志、配置、路径、剪贴板、主题、Java 扫描、HTTP 代理等建立平台抽象。当前计划 B 的 2.5–3.5 人月是否覆盖这部分拆分，应在 Phase 0 结束前重新核对。

## 4. 平台相关实现

以下文件包含注册表、DPAPI/CNG、P/Invoke 或 Windows Shell 依赖，是 Linux 平台化的首批候选：

- `App/Paths.cs`、`App/Secrets.cs`、`App/Metadata.cs`
- `App/Essentials/PromoteService.cs`、`App/Essentials/TelemetryService.cs`
- `App/IoC/LifecycleProcess.cs`、`App/Tools/DependencyCheckService.cs`
- `IO/Net/Http/HttpProxyManager.cs`
- `Link/EasyTier/ETInfoProvider.cs`、`Link/Lobby/LobbyController.cs`、`Link/Natayark/NatayarkProfileManager.cs`
- `Link/Scaffolding/EasyTier/CliNetTest.cs`、`Link/Scaffolding/EasyTier/EasyTierEntity.cs`
- `Minecraft/Java/Scanner/DefaultPathsScanner.cs`、`MicrosoftStoreJavaScanner.cs`、`RegistryJavaScanner.cs`、`WhereCommandScanner.cs`
- `UI/SystemDialogs.cs`、`UI/Theme/SystemThemeHelper.cs`
- `Utils/OS/AumidHelper.cs`、`EnvironmentInterop.cs`、`ProcessInterop.cs`、`SystemTheme.cs`
- `Utils/Secret/EncryptHelper.cs`

当前路径模型（`Paths.cs`）直接使用 `Environment.SpecialFolder.ApplicationData/LocalApplicationData` 和固定目录名，未体现 XDG 规范；`Secrets.cs` 走环境变量/构建期注入，尚无运行时 `ISecretStore`。

## 5. 构建与测试基线

`dotnet build "Plain Craft Launcher 2.slnx" -c Debug -p:Platform=x64`：

- 结果：**0 errors，5,382 warnings**
- 耗时：约 2 分 25 秒（本机，含还原）
- 说明：警告数量很大，迁移期间需要单独的“不得新增警告”基线，否则无法从构建日志发现回归。

测试项目未纳入 slnx 构建。`dotnet build PCL.Core.Test/PCL.Core.Test.csproj -c Debug -p:Platform=x64` 可通过（0 errors / 18 warnings）。

测试运行基线：

```powershell
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Debug --no-build
```

结果：**209 passed / 47 failed / 0 skipped / 256 total**，耗时约 2 分 7 秒。

失败集中在：

- SQLite 缓存/哈希测试的 `TestCleanup` 文件锁：`test_cache.db`、`.hash_cache.db` 被占用，无法删除。
- `Basics` 静态构造在测试宿主中拿不到入口程序集资源，导致配置存储初始化失败。
- `EventBusService` 在未初始化生命周期上下文时调用。
- 部分缓存内容断言失败（长度 0、元素数量不一致）。

这些失败在迁移前就存在，不能算作 V2 回归；但必须在 Phase 1 前完成分类（环境问题/测试隔离问题/真实缺陷），否则 QA 无法建立可信基线。Phase 0 需要补一条可重复的测试命令并固定运行环境。

## 6. CI 现状

- `reusable-build.yml` 在 `ubuntu-latest` 上交叉发布 `win-x64` / `win-arm64`，只验证 Windows 目标能编译。
- 没有 Linux 运行时、Avalonia、IME、AppImage 或桌面环境测试。
- 没有性能基准任务。

## 7. Phase 0 需要补齐的前置定义

1. **P0 页面清单未冻结。** 需要列出页面名、现有代码量、依赖服务、验收人。
2. **“Core”边界未冻结。** 建议在 Phase 0 决定 `PCL.Core`、`PCL.UI`、`PCL.Platform.*` 的项目划分，否则 B/E/F 的工作包无法准确估算。
3. **Linux 验证环境缺失。** 当前主机没有 WSL、Docker 或 Linux 桌面；IME、libsecret、Forge、AppImage、原版启动必须在 Ubuntu 22.04/24.04 实机或 VM 上执行。
4. **旧数据兼容样本未准备。** 需要一份 Windows 旧版配置、实例目录、DPAPI 密钥和日志样本，用于 Phase 1/2 兼容测试。

## 8. 复现命令

```powershell
# 代码规模
Get-ChildItem PCL.Core -Recurse -File -Filter *.cs |
  Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
  ForEach-Object { Get-Content $_.FullName } | Measure-Object -Line

# WPF 引用文件
Get-ChildItem PCL.Core -Recurse -File -Filter *.cs |
  Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
  Select-String -Pattern 'using System\.Windows|using Microsoft\.Win32|using System\.Drawing' -List

# 测试数量
Get-ChildItem PCL.Core.Test -Recurse -File -Filter *.cs |
  Select-String -Pattern '\[TestMethod\]' | Measure-Object

# 构建
dotnet build "Plain Craft Launcher 2.slnx" -c Debug -p:Platform=x64 --nologo
```
