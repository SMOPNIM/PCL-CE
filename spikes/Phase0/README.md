# Phase 0 技术探针

本目录存放 Gate 0 期间的验证工程。**不加入 `Plain Craft Launcher 2.slnx`，不进入发布包。**

| 探针 | 验证项 | 状态 |
|---|---|---|
| [AvaloniaProbe](AvaloniaProbe/README.md) | 1. Avalonia 版本、启动、Linux IME | Wayland + fcitx5 实测通过；X11/ibus 未覆盖 |
| [SecretStoreProbe](SecretStoreProbe/README.md) | 3. Secret Service 与 AES 文件 fallback | 两种路径探针实测通过；仅为 Phase 0 探针，不代表生产实现 |
| [AppImageProbe](AppImageProbe/README.md) | 8. AppImage/FUSE 启动机制 | Ubuntu 22.04.5 正常 FUSE 与显式 extract-and-run 无头 smoke 通过；非 PCL 产物，未在无 FUSE 主机验证 |
| [OAuthLoopbackProbe](OAuthLoopbackProbe/README.md) | 2. Microsoft browser OAuth/PKCE/loopback | 固定 redirect 版本已部署到 Ubuntu；等待登记 `http://localhost:8400/` 后桌面交互测试 |

后续探针建议：

- `LinuxSecretProbe`：libsecret 可用/不可用/fallback。
- `AuthLoopbackProbe`：系统浏览器 + loopback 回调。
- `ForgeLinuxProbe`：Forge/NeoForge 安装器冒烟。
- `DownloadServiceProbe`：代理、重试、限速、校验和、镜像切换。
- `WindowDecorationProbe`：X11/Wayland 自定义标题栏与系统装饰降级。
- `ModPerfProbe`：200 Mod 首屏/完整解析/滚动基准。
- `AppImageProbe`：无 FUSE 回退与干净 Ubuntu 运行。

每个探针必须写明：通过标准、运行环境、命令、原始结果、结论。探针结果回填 `docs/cross-platform-v2/phase-0/README.md`。
