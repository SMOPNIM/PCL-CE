# 验证 3 探针证据：Secret Service 与 fallback

> 日期：2026-11-01
> 环境：Ubuntu 22.04.5 LTS / GNOME Wayland / 用户 D-Bus
> 探针：`spikes/Phase0/SecretStoreProbe`（本地工作树构建，非生产代码）
> 结论：**探针两种路径均通过；生产集成未验证。** Phase 0 验证 3 保持 `PARTIAL`。

## Secret Service 路径

使用 `secret-tool` 连接 GNOME Keyring 的用户 D-Bus。首次测试时 VM 处于锁屏状态，操作等待；解锁后重复完整存取与清理成功。

```text
STORE_OK backend=SecretService fingerprint=28631757986A
READ_OK backend=SecretService fingerprint=28631757986A
CLEAR_OK backend=SecretService
SECRET_SERVICE_RETRY_CLEANUP_OK
```

存取指纹相同；测试条目已从 keyring 清理。

## 无 Secret Service 路径

不停止桌面 keyring，而是对探针进程设置指向不存在 socket 的 `DBUS_SESSION_BUS_ADDRESS`，模拟 Secret Service 不可达：

```text
WARNING: Secret Service unavailable (...); using AES-256-GCM file fallback.
STORE_OK backend=AesFallback fingerprint=E095F5D2BD6E
WARNING: Secret Service unavailable (...); using AES-256-GCM file fallback.
READ_OK backend=AesFallback fingerprint=E095F5D2BD6E
700 /home/user/.local/share/pcl-phase0-secret-store-probe
600 .../228ccb12...enc
600 .../228ccb12...key
CLEAR_OK backend=AesFallback
FALLBACK_CLEANUP_OK
```

存取指纹相同；目录为 `0700`，密钥和密文文件均为 `0600`；测试文件已删除。探针打印了明确 fallback 警告，未崩溃。

## 限制

- 这是隔离的 Phase 0 技术探针；仓库生产代码目前仍没有 `ISecretStore` 或 Linux fallback 接入。
- fallback 目录内同时保存随机密钥和密文；本测试验证计划中的权限和往返行为，不证明抵抗同一用户读取，也不构成生产安全评审。
- 未停止或修改正在运行的 GNOME Keyring 服务。无服务情景通过隔离 D-Bus 地址模拟。
