# Linux 后续验证准备检查

> 日期：2026-11-01
> 环境：Ubuntu 22.04.5 LTS / x86_64 / GNOME Wayland
> 检查方式：SSH 环境检查、隔离探针准备和官方 Forge 安装器尝试。
> 结论：本记录区分环境探测、Phase 0 探针和产品 Gate；探针通过不代表生产功能通过。

## 环境检查结果

| 项目 | 结果 |
|---|---|
| 桌面 Secret Service | `gnome-keyring-daemon` 正在运行；用户 D-Bus 可见 `org.freedesktop.secrets` |
| SSH 是否隔离 keyring | 否。SSH 下 `busctl --user` 仍能看到桌面 Secret Service，因此不能用当前 SSH shell 代表无 keyring 环境 |
| Secret Service 命令行工具 | 初检时未找到 `secret-tool`；用户随后安装 `libsecret-tools`，并完成了 Secret Service 探针测试 |
| Java | OpenJDK 17.0.20 与 21.0.12 均已安装；默认 `PATH` 指向 21 |
| AppImage | 初检时未找到；随后已生成 AvaloniaProbe AppImage 并完成两种无头 smoke，详见单独证据 |
| FUSE | `/dev/fuse` 和 `fusermount3` 存在；尚未进行 FUSE 禁用环境测试 |
| Minecraft 数据 | 未发现 `~/.minecraft` 或 `~/.local/share/minecraft` |
| 应用测试入口 | 已有 AvaloniaProbe、SecretStoreProbe、AppImageProbe 和 OAuthLoopbackProbe；均为 Phase 0 探针。生产代码仍无 Linux `ISecretStore`、跨平台登录集成或 Linux Minecraft 启动链 |

## 各验证项状态

### 验证 2：微软登录

**BLOCKED，Redirect URI 与 Microsoft App Registration 不匹配。** 用户从 GNOME 桌面运行最新 Linux 自包含 [OAuthLoopbackProbe](../../../spikes/Phase0/OAuthLoopbackProbe/README.md) 后，Microsoft 返回 `invalid_request`，提示请求中的 `redirect_uri` 未登记。探针默认 URI 为 `http://localhost:8400/`，部署路径为 `/home/user/phase0-oauth-loopback-probe-20261101/`；部署 DLL SHA-256 为 `91a27a16d9a862ab068060d33efa059fbb0a89af3778f8cf415a94c29170cc1d`，与本地固定 URI 构建一致。需确认所用 Client ID 对应的应用注册中，在 **Mobile and desktop applications** 平台登记的 URI 与实际请求逐字符一致（含 `localhost`、端口 `8400` 和尾部 `/`），并确认运行时没有设置不同的 `PHASE0_MS_REDIRECT_URI`。修正后再从 GNOME 桌面终端重跑。先前动态端口版本超时也不代表当前构建结果。不得通过聊天传递账号密码、授权码或 token；该探针不持久化 token。

### 验证 3：libsecret fallback

初检时 **BLOCKED**：仓库没有 `ISecretStore` 和 Linux fallback 实现。随后新增了仅用于 Phase 0 的探针，用户安装 `libsecret-tools` 后已分别验证 Secret Service 和无 D-Bus fallback 路径。当前 **探针测试通过、生产集成未验证**，因此整体状态为 `PARTIAL`。详见[验证 3 实测证据](2026-11-01-secret-store-ubuntu.md)。

测试没有停止 keyring 服务；无 Secret Service 场景通过给探针设置不可用的 D-Bus 地址模拟。

### 验证 4：Forge Linux 冒烟

**BLOCKED。** Java 17 与 Forge 1.20.1-47.4.0 安装器已准备，但安装器在 TLS 证书检查阶段等待超时，未生成版本。详见[Forge 安装器尝试记录](2026-11-01-forge-installer-ubuntu.md)。

### 验证 8：AppImage

AvaloniaProbe AppImage 的正常启动和显式 extract-and-run 无头 smoke 均通过；用户另确认两条 GUI 路径都成功打开窗口。该产物不是 PCL 产物，且 VM 有 `/dev/fuse`，无 FUSE 主机测试仍未完成，因此整体 **PARTIAL**。详见[AppImage 探针证据](2026-11-01-appimage-probe-ubuntu.md)。

### 验证 9：原版启动

**BLOCKED。** 仓库没有 Linux Minecraft 启动链，VM 没有 Minecraft 1.20.1 游戏数据。Java 17/21 均已安装。准备 Linux 启动器和用户自己的合法游戏账号后，才能验收到主菜单。

## 检查命令摘要

```bash
cat /etc/os-release
ldd --version
java -version
command -v secret-tool
busctl --user list
pgrep -af gnome-keyring-daemon
find /home/user /tmp -maxdepth 4 -type f -iname '*.AppImage'
ls -l /dev/fuse
command -v fusermount3
```

本记录只描述检查时的环境与代码可测性，不代表上述功能已实现或已通过测试。
