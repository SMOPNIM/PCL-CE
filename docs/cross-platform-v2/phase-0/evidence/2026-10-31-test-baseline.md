# 测试基线证据（迁移前）

> 日期：2026-10-31
> 提交：`3ce29b18`
> 主机：Windows 10.0.26300 / x64 / .NET SDK 10.0.402
> 结论：测试项目可构建；全量测试 **209 passed / 47 failed / 0 skipped / 256 total**，失败为迁移前已有问题，需在 Phase 1 前分类。

## 1. 命令

```powershell
dotnet build PCL.Core.Test/PCL.Core.Test.csproj -c Debug -p:Platform=x64
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Debug --no-build `
  --nologo --logger "console;verbosity=minimal"
```

构建结果：0 errors / 18 warnings。
测试结果：耗时约 2 分 7 秒，退出码 1。

## 2. 失败分类

| 类别 | 估计数量 | 典型错误 |
|---|---:|---|
| 缓存/哈希 SQLite 清理 | 30+ | `test_cache.db`、`.hash_cache.db` 被占用，`TestCleanup` 删除失败 |
| 缓存内容断言 | 5+ | 读取长度为 0、元素数量不一致 |
| 静态初始化/生命周期 | 2 | `Basics` 入口程序集资源为空；`EventBusService` Not initialized |
| 本地化 | 2 | 语言字典缺键、字体配置 |
| 网络/Toast/其他 | 3+ | `TestSrvQuery`、`TestEncoding`、`TestToast` |

## 3. 失败用例

```text
AllLanguageDictionariesShouldContainBaseKeys
CacheFileAsync_ShouldCacheFileStream
CacheFileAsync_ThenRetrieve_ShouldReturnExactContent
ClearAsync_ShouldCleanupFileReferences
ClearAsync_ShouldRemoveAllCachedItems
ConcurrentSetGet_ShouldBeThreadSafe
DeleteByGroupAsync_ShouldRemoveAllItemsInGroup
DeleteByGroupAsync_WithFileReferences_ShouldReleaseFiles
DeleteByTagAsync_ShouldRemoveItemsWithTag
DeleteExistingKey_ShouldRemoveFromCache
DeleteExpiredAsync_ShouldRemoveExpiredItems
DeleteNonExistentKey_ShouldReturnFalse
ExistsForExpiredKey_ShouldReturnFalse
ExistsForNonExistentKey_ShouldReturnFalse
ExistsForValidKey_ShouldReturnTrue
FontProfileShouldFollowCultureGlyphStandard
GetCachedFilePathAsync_ShouldReturnFilePath
GetNonExistentKey_ShouldReturnMiss
GetStatsAsync_HitRate_ShouldBeCalculatedCorrectly
GetStatsAsync_ShouldReturnAccurateStats
MixedInlineAndFileStorage_ShouldCoexist
RetrieveAsync_ShouldReturnStoredContent
SetAndGetInt_ShouldReturnCachedValue
SetAndGetObject_ShouldReturnCachedValue
SetAndGetString_ShouldReturnCachedValue
SetLargeData_ShouldUseFileRefStorage
SetMultipleItemsInGroup_ShouldAllHaveSameGroup
SetSmallData_ShouldUseInlineStorage
SetWithAbsoluteExpiration_ShouldExpireAfterTimeout
SetWithNeverExpirePolicy_ShouldNotExpire
Stop_ShouldPersistQueuedWrite
StoreLargeFile_ShouldWorkCorrectly
StoreWithCompressionDisabled_ShouldStoreUncompressed
TestCache
TestConcurrentDeleteAndQuery
TestConcurrentReadDuringWrite_DoesNotBlock
TestEncoding
TestFileModifiedDuringConcurrentAccess
TestManyFiles_SingleDB_AllAlgorithms_Concurrent
TestManyFiles_SingleDB_SameAlgo_Concurrent
TestMixedCacheHitAndCompute_Concurrent
TestMultipleInstances_SharedDB_Concurrent
TestSingleFile_SingleDB_AllAlgorithms_Concurrent
TestSingleFile_SingleDB_SameAlgo_HeavyConcurrent
TestSrvQuery
TestToast
Unsubscribe_Prevents_Handler_Call
```

## 4. 处理建议

1. 先把测试分为“纯单元 / 需要文件系统 / 需要网络 / 需要桌面或入口程序集”四类。
2. 对 SQLite 测试增加连接释放等待和重试清理，或改用临时目录 + 唯一数据库名，避免并行测试互锁。
3. 为 `Basics`、`EventBusService` 提供测试宿主初始化，不依赖入口程序集。
4. 将网络测试标记为可选/集成测试，默认 CI 不跑。
5. 建立迁移前通过率基线；Phase 1 不允许降低通过数。

在分类完成前，这 47 个失败不能作为 V2 回归信号，也不能被忽略。
