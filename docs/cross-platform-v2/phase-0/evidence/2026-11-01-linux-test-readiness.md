# Linux 后续验证准备检查

> 日期：2026-11-01
> 环境：Ubuntu 22.04.5 LTS / x86_64 / GNOME Wayland
> 检查方式：SSH 只读检查；未停止系统服务、未安装软件、未运行游戏或安装器。
> 结论：验证 2、3、4、8、9 当前受实现或测试产物缺失阻塞；这不是功能失败结论。

## 环境检查结果

| 项目 | 结果 |
|---|---|
| 桌面 Secret Service | `gnome-keyring-daemon` 正在运行；用户 D-Bus 可见 `org.freedesktop.secrets` |
| SSH 是否隔离 keyring | 否。SSH 下 `busctl --user` 仍能看到桌面 Secret Service，因此不能用当前 SSH shell 代表无 keyring 环境 |
| Secret Service 命令行工具 | 初检时未找到 `secret-tool`；用户随后安装 `libsecret-tools`，并完成了 Secret Service 探针测试 |
| Java | OpenJDK 21.0.12；当前 `PATH` 未发现 Java 17 |
| AppImage | 在 `/home/user` 和 `/tmp` 的检查范围内未找到 `.AppImage` 文件 |
| FUSE | `/dev/fuse` 和 `fusermount3` 存在；尚未进行 FUSE 禁用环境测试 |
| Minecraft 数据 | 未发现 `~/.minecraft` 或 `~/.local/share/minecraft` |
| 应用测试入口 | 初检时只有 Avalonia IME 探针；随后新增了仅供 Phase 0 使用的 SecretStoreProbe。仓库生产代码仍无 Linux `ISecretStore`、跨平台登录实现、Linux Minecraft 启动链或 AppImage 产物 |

## 各验证项状态

### 验证 2：微软登录

**BLOCKED。** 当前仓库没有跨平台登录测试入口，尚未准备 OAuth 测试应用。准备客户端配置后，需由用户在虚拟机中亲自授权；不得通过聊天传递账号密码、授权码或 token。

### 验证 3：libsecret fallback

初检时 **BLOCKED**：仓库没有 `ISecretStore` 和 Linux fallback 实现。随后新增了仅用于 Phase 0 的探针，用户安装 `libsecret-tools` 后已分别验证 Secret Service 和无 D-Bus fallback 路径。当前 **探针测试通过、生产集成未验证**，因此整体状态为 `PARTIAL`。详见[验证 3 实测证据](2026-11-01-secret-store-ubuntu.md)。

测试没有停止 keyring 服务；无 Secret Service 场景通过给探针设置不可用的 D-Bus 地址模拟。

### 验证 4：Forge Linux 冒烟

**BLOCKED。** VM 当前 `PATH` 仅确认到 Java 21，未准备 Java 17、目标 Forge 安装器和可启动的 PCL Linux 启动流程。选定 Minecraft/Forge 版本并准备 Java 17 后，先测安装器能否生成版本，再通过 Linux 启动器做实际启动验收。

### 验证 8：AppImage

**BLOCKED。** 尚无 PCL AppImage 产物，正常运行和 `--appimage-extract-and-run` 都无法执行。VM 当前具备 FUSE，后续还需准备隔离的无 FUSE 环境做回退测试。

### 验证 9：原版启动

**BLOCKED。** 仓库没有 Linux Minecraft 启动链，VM 也没有 Minecraft 1.20.1 游戏数据。准备 Linux 启动器、Java 21 和用户自己的合法游戏账号后，才能验收到主菜单。

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
