# Phase 0 执行板

> 状态：**进行中，Gate 0 未通过**
> 周期：4–5 周
> 开始日期：2026-10-31
> 目标：用可复现证据消除 Avalonia、Linux 输入法、认证、密钥存储、启动链、打包和性能风险，然后决定是否进入 Phase 1。

## 1. Gate 0 规则

强制通过项：**1、2、3、4、8、9**。
任一项失败，暂停进入 Phase 1，先缩范围或调整方案。

范围触发项：Linux IME、原版启动、Forge Linux、AppImage、200 Mod 列表、自定义标题栏中任一项失败，也要暂停 Phase 1 评审。

状态定义：

| 状态 | 含义 |
|---|---|
| `NOT STARTED` | 尚未开始 |
| `IN PROGRESS` | 已开始，未取得结论 |
| `PARTIAL` | 部分通过，关键场景未验证 |
| `PASS` | 通过，证据可复现 |
| `FAIL` | 未通过，需调整范围或方案 |
| `BLOCKED` | 缺环境/账号/依赖，无法继续 |

## 2. 验证矩阵

| # | 验证项 | 通过标准 | 环境 | 状态 |
|---|---|---|---|---|
| 1 | Avalonia 版本与 Linux IME | 确认 12.1.4 为稳定版；fcitx5/ibus 至少一个可中文输入 | Windows + Ubuntu 桌面 | **PASS** |
| 2 | 微软登录跨平台 | 系统浏览器打开、loopback 回调成功、token 可存可取 | Windows + Linux | **BLOCKED** |
| 3 | libsecret fallback | 无 keyring 环境下能启动、能提示、不崩溃 | Linux | **PARTIAL** |
| 4 | Forge/NeoForge Linux 冒烟 | 至少 Forge 安装器在 Linux 跑通并生成可启动版本 | Linux + Java | **BLOCKED** |
| 5 | 下载服务 | 代理、重试、限速、校验和、镜像切换可用 | Windows（可先行） | NOT STARTED |
| 6 | 自定义标题栏 | GNOME X11/Wayland 至少一个可用；否则接受系统装饰降级 | Linux GNOME | NOT STARTED |
| 7 | 200 Mod 列表 | 首屏 < 1s，完整解析 < 3s | 基准硬件 | NOT STARTED |
| 8 | AppImage | 干净 Ubuntu 可运行；无 FUSE 时 `--appimage-extract-and-run` 可用 | Ubuntu 22.04 | **PARTIAL** |
| 9 | 原版启动 | 1.20.1 在 Ubuntu 启动成功 | Ubuntu + Java 21 | **BLOCKED** |
| 10 | 性能基线 | 记录冷启动、内存、扫描、滚动数据 | 基准硬件 | NOT STARTED |

### 验证 1 的证据

Windows 版本、构建和无头启动证据见 [2026-10-31 验证记录](evidence/2026-10-31-avalonia-12.1.4.md)。Ubuntu 桌面 IME 实测见 [2026-11-01 验证记录](evidence/2026-11-01-avalonia-ime-ubuntu.md)。

已完成：

- NuGet 上 `Avalonia` / `Avalonia.Desktop` / `Avalonia.Headless` 最新稳定版均为 **12.1.4**；GitHub Release 发布于 2026-10-09。
- 新增 `spikes/Phase0/AvaloniaProbe`，目标框架 `net10.0`，引用 Avalonia 12.1.4。
- Windows：Debug 构建通过；GUI 进程启动后存活；`--smoke` 无头启动输出 `Avalonia headless startup: OK`。
- Windows 交叉发布 `linux-x64` 成功。
- 已添加手动 CI 工作流 `phase0-linux-smoke.yml`，在 Ubuntu runner 上执行无头启动。
- Ubuntu 22.04.5 LTS / GNOME Wayland / fcitx5 5.0.14：主窗口自动聚焦和中文输入正常；首次打开的对话框无需切换窗口即可输入中文。

验证范围说明：

- 本次覆盖 GNOME Wayland + fcitx5；X11、ibus 未测。验证 1 最低标准及本次实测组合下的对话框体验标准已通过，不代表其他桌面组合均已验证。
- Avalonia 上游仍有未关闭的 Linux X11 DBus IME 问题（AvaloniaUI/Avalonia#22315）；未覆盖平台风险继续跟踪。
- 验证 1 记为 `PASS`；Gate 0 仍因验证 2、3、4、8、9 未完成而未通过。

验收补充：最低门槛是“fcitx5 或 ibus 至少一个可中文输入”；但登录/设置等**首次打开的对话框**如果必须切换窗口才能输入，应记为 P0 体验缺陷并纳入范围评审。

### 验证 2、3、4、8、9 的准备检查

2026-11-01 在 Ubuntu 22.04.5 虚拟机完成探针准备和部分实测，具体检查记录见[测试准备检查证据](evidence/2026-11-01-linux-test-readiness.md)。验证 3 的独立探针结果见[验证 3 实测证据](evidence/2026-11-01-secret-store-ubuntu.md)。验证 2、4、8、9 尚未通过：

- 验证 2：仅供 Phase 0 的 [OAuthLoopbackProbe](../../../spikes/Phase0/OAuthLoopbackProbe/README.md) 已按固定 loopback URI `http://localhost:8400/` 重建并部署到 Ubuntu；用户运行时 Microsoft 返回 `invalid_request`，提示请求 URI 未在该 Client ID 对应的应用注册中登记。需逐字符核对 **Mobile and desktop applications** 下的 URI（包括尾部 `/`）以及 `PHASE0_MS_REDIRECT_URI` 环境变量，修正后重跑授权与 token exchange。该探针不持久化 token，也不验证生产登录/密钥存储集成。
- 验证 3：仅用于 Phase 0 的 [SecretStoreProbe](../../../spikes/Phase0/SecretStoreProbe/README.md) 已通过桌面 Secret Service 与无 D-Bus fallback 存取测试，权限符合要求；生产代码尚无 `ISecretStore` 或 Linux fallback，因此整体状态为 `PARTIAL`。
- 验证 4：Ubuntu 已安装 Java 17/21，官方 Forge 1.20.1-47.4.0 安装器已准备；安装器在 TLS 证书检查阶段等待超过 10 分钟并超时，未生成版本。详见[Forge 安装器尝试记录](evidence/2026-11-01-forge-installer-ubuntu.md)。
- 验证 8：已用 AvaloniaProbe 生成 AppImage 并通过正常 FUSE/显式 extract-and-run 无头 smoke；用户确认两条 GUI 路径均能成功打开窗口。该探针不是 PCL 产物，VM 有 `/dev/fuse`，无 FUSE 主机和 PCL 打包仍未验证。详见[AppImage 探针证据](evidence/2026-11-01-appimage-probe-ubuntu.md)。
- 验证 9：仓库没有 Linux 启动链，VM 也没有 Minecraft 1.20.1 安装数据。游戏启动需使用用户自己的合法账号，不在此收集或传递凭据。

## 3. 环境缺口

| 需求 | 现状 | 影响 |
|---|---|---|
| Ubuntu 22.04 桌面 | Ubuntu 22.04.5 LTS GNOME Wayland 虚拟机可用 | 可继续验证 2、3、4、6、8、9 |
| fcitx5 + 中文输入法、ibus 备选 | fcitx5 5.0.14 已验证；ibus 未测 | 验证 1 其他组合 |
| GNOME X11 + Wayland 两会话 | Wayland 已验证；X11 未测 | 验证 1、6 |
| Java 8/17/21 矩阵 | 未准备 | 验证 4、9 |
| 4 核 / 8GB / SATA SSD 基准机 | 未固化 | 验证 7、10 |
| 200 Mod 测试库 | 未准备 | 验证 7 |
| 微软 OAuth 应用登记 | 已有应用；需登记 `http://localhost:8400/` 并从 VM 桌面登录 | 验证 2 |
| Windows 旧版数据样本 | 未准备 | Phase 1/2 兼容回归 |

## 4. 本轮已完成的启动工作

- 固化估算口径、WBS、日历和 Gate 规则（`docs/cross-platform-v2/`）。
- 完成当前代码基线审计：Core 46,361 行、WPF 引用 67 个文件、现有构建 0 error / 5,382 warnings。
- 复测并分类 Windows 测试基线：256 passed / 1 failed / 257 total；已修复缓存生命周期、SQLite 清理、过期精度、哈希流内容、编码检测及测试初始化问题。唯一剩余失败为 `zh-TW.xaml` 缺少 57 个新资源键（[复测证据](evidence/2026-11-01-test-rerun.md)；[原始迁移前基线](evidence/2026-10-31-test-baseline.md)）。
- 建立 Avalonia 12.1.4 探针工程和 Linux 无头 CI 入口。
- 确认当前代码库没有目标平台接口、没有 Avalonia 依赖、Linux 运行时验证为空。

本轮修复了测试复测暴露的 Core 缺陷，但没有开始跨平台架构迁移或 Phase 1 功能开发；Gate 0 仍未通过。

## 5. 下一步（按优先级）

1. 在 Ubuntu GNOME 桌面登记 `http://localhost:8400/` 后运行已部署的 OAuth 探针；完成微软授权、loopback 回调与 token exchange 检查。
2. 在桌面运行 AppImage 正常 GUI 与显式 `--appimage-extract-and-run` GUI，记录窗口结果；随后安排无 FUSE 环境验证。
3. 排查 Java 到 Forge Maven 的 TLS 握手等待；不得关闭证书校验。修复网络后重跑 Forge 安装器。
4. 在 Ubuntu 桌面补测 GNOME X11、ibus 组合，记录与已通过的 Wayland + fcitx5 结果差异。
5. 如需扩充 IME 覆盖，在 X11/ibus 与 Wayland/fcitx5 下分别记录主窗口、首次打开对话框、切换窗口后的中文输入行为。
6. 冻结 P0 页面清单和 `PCL.Core` / `PCL.UI` / `PCL.Platform.*` 边界，回填 WBS。
7. 补齐 `zh-TW.xaml` 缺少的 57 个资源键并重跑全量测试；继续保留真实失败，不通过 quarantine 隐藏。
8. 启动验证 5（下载服务）和验证 7/10（性能基准）的 Windows 侧工作，这两项不依赖 Linux。
9. 准备旧版数据样本；每周更新执行板，并在 Gate 0 评审时附日志、截图和命令。

## 6. 证据约定

- 证据文件放在 `docs/cross-platform-v2/phase-0/evidence/`。
- 文件名：`YYYY-MM-DD-<validation>-<short-name>.md`。
- 必须包含：提交 SHA、OS/桌面/输入法版本、命令、原始输出摘要、结论、复现步骤。
- 截图和日志如体积较大，放到 `artifact/phase0/`，仓库内只保留索引和关键摘要。

## 7. 风险登记

| 风险 | 概率 | 影响 | 应对 | 状态 |
|---|---|---|---|---|
| Linux IME 受 Avalonia #22315 影响 | 中 | 高 | Phase 0 实测；准备延迟聚焦、启动强制焦点、XWayland 降级；失败则暂停 Phase 1 | 跟踪中 |
| Core 去 WPF 复杂度高于估算 | 高 | 中高 | Phase 0 结束前拆分 `PCL.Core` / `PCL.UI` / `PCL.Platform.*`，B 包暂定上调至 3.5–5.0 人月 | 已登记 |
| QA 基线仍有失败 | 中 | 中 | SQLite/哈希/编码/测试宿主问题已处理；补齐 zh-TW 的 57 个新资源键并重跑全量测试 | 剩余 1 项 |
| Linux 桌面组合覆盖不足导致 Gate 0 证据不完整 | 中 | 高 | Ubuntu 22.04.5 GNOME Wayland 已可用；继续补测 X11、ibus 和其余 Linux Gate 项 | 跟踪中 |
| 自定义标题栏在 Wayland 不可用 | 中 | 中 | Phase 0 实测；失败则按计划降级为系统装饰并更新 P0 验收 | 待验证 |
| AppImage 无 FUSE 环境不可运行 | 中 | 中 | Phase 0 实测 `--appimage-extract-and-run`；准备 libfuse2 文档 | 待验证 |
| `Avalonia.Diagnostics` 无 12.x 版本 | 高 | 低 | 不依赖该包；诊断使用日志/无头测试/其他 12.x 工具 | 已登记 |

风险登记每次 Gate 评审更新；新增高风险项必须同步到 WBS/日历影响评估。
