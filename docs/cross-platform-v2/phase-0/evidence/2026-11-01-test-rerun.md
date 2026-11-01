# Phase 0 Windows 测试基线复测

> 日期：2026-11-01
> 基线提交：`4905d622`（测试时包含未提交的本地修复）
> 环境：Windows 10.0.26300 / x64 / .NET SDK 10.0.402
> 结论：全量测试 **256 passed / 1 failed / 0 skipped / 257 total**。原 47 个失败已分类并修复其主要测试隔离与 Core 缺陷；剩余失败是繁体中文资源缺少 57 个新键。

## 全量测试

```powershell
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Debug -p:Platform=x64 --no-restore --nologo --logger "console;verbosity=minimal"
```

结果：退出码 1，耗时 1 分 39 秒。唯一失败：

| 用例 | 分类 | 结果与后续 |
|---|---|---|
| `AllLanguageDictionariesShouldContainBaseKeys` | 本地化资源缺失 | `zh-TW.xaml` 缺少 57 个近期新增键，属于真实产品内容缺口；由本地化维护者补齐繁体中文翻译后重跑。 |

该失败不属于跨平台启动阻塞，但全量测试尚未全绿，不能把它忽略或 quarantine。

## 已复现并修复

| 原失败类别 | 根因 | 修复 |
|---|---|---|
| `CacheServiceTest` SQLite 文件锁 | `CacheEvictionService.Stop()` 取消任务但未等待后台循环退出；测试在连接池仍持有数据库文件时删除目录 | 增加异步停止并等待循环完成；测试清理指定连接池后删除临时目录。 |
| 缓存过期断言 | SQL `datetime()` 丢失毫秒精度，且 `DeleteExpiredAsync` 忽略传入时间 | 使用 `julianday()` 比较 ISO 时间并保留调用方 `now`。 |
| 哈希文件内容为空/长度不匹配 | `HashStorage.PutAsync` 为计算哈希读完可寻址流后，没有回到流首再写入；不可寻址流也无法正确处理 | 哈希后重置可寻址流；不可寻址输入先暂存到存储目录内的临时文件，并新增回归测试。 |
| `HashCacheTest` SQLite 清理锁 | 测试删除仍有 SQLite 池连接的临时目录 | 清理对应测试数据库连接池。 |
| `TestEncoding` | Core 启动时注册 CodePages，但单测没有启动生命周期；UTF-8 检测实际未读取样本，且 BOM 匹配错误 | EncodingDetector 自行注册 provider、按样本严格验证 UTF-8/GB2312，并修复 BOM 检查。 |
| 字体策略测试初始化失败 | 静态初始化阶段创建 WPF `pack:` URI，即使仅调用不需要 WPF 的文化解析方法也会失败 | 将 URI 延迟到实际构建 WPF 字体时创建。 |
| `Unsubscribe_Prevents_Handler_Call` | 无订阅者路径访问尚未初始化的服务级生命周期 Context | 使用始终可用的系统生命周期 Context 输出该诊断日志。 |
| `Stop_ShouldPersistQueuedWrite` | Debug 跟踪开关初始化 `Basics`，测试入口程序集没有 launcher metadata resource | 仅从进程命令行读取 Debug 跟踪标志，不再触发无关的 `Basics` 静态初始化。 |
| `TestSrvQuery` | 测试依赖已无 SRV 记录的临时域名 | 改用实测有 SRV 记录的 `_xmpp-server._tcp.jabber.org`。 |
| 混合过期集成测试 | 100/200/400 ms TTL 容易被 SQLite/机器调度耗时吞掉，产生时间相关误报 | 将 TTL 和检查间隔扩大到秒级并保留检查窗口。 |

## 定向验证

```powershell
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Debug -p:Platform=x64 --no-restore --nologo --logger "console;verbosity=minimal" --filter "FullyQualifiedName~CacheServiceTest|FullyQualifiedName~HashCacheTest|FullyQualifiedName~FileCacheStorageTest|FullyQualifiedName~EncodingDetectorTest"
```

结果：**54 passed / 0 failed**。

```powershell
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Debug -p:Platform=x64 --no-restore --nologo --logger "console;verbosity=minimal" --filter "FullyQualifiedName~CacheSystemIntegrationTest"
```

结果：**11 passed / 0 failed**。

剩余测试门槛：补齐 `zh-TW.xaml` 的 57 个资源键并再次运行全量测试。测试修复不会改变 Phase 0 Gate 0 的未通过结论。
