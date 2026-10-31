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
| 1 | Avalonia 版本与 Linux IME | 确认 12.1.4 为稳定版；fcitx5/ibus 至少一个可中文输入 | Windows + Ubuntu 桌面 | **PARTIAL** |
| 2 | 微软登录跨平台 | 系统浏览器打开、loopback 回调成功、token 可存可取 | Windows + Linux | NOT STARTED |
| 3 | libsecret fallback | 无 keyring 环境下能启动、能提示、不崩溃 | Linux | NOT STARTED |
| 4 | Forge/NeoForge Linux 冒烟 | 至少 Forge 安装器在 Linux 跑通并生成可启动版本 | Linux + Java | NOT STARTED |
| 5 | 下载服务 | 代理、重试、限速、校验和、镜像切换可用 | Windows（可先行） | NOT STARTED |
| 6 | 自定义标题栏 | GNOME X11/Wayland 至少一个可用；否则接受系统装饰降级 | Linux GNOME | NOT STARTED |
| 7 | 200 Mod 列表 | 首屏 < 1s，完整解析 < 3s | 基准硬件 | NOT STARTED |
| 8 | AppImage | 干净 Ubuntu 可运行；无 FUSE 时 `--appimage-extract-and-run` 可用 | Ubuntu 22.04 | NOT STARTED |
| 9 | 原版启动 | 1.20.1 在 Ubuntu 启动成功 | Ubuntu + Java 21 | NOT STARTED |
| 10 | 性能基线 | 记录冷启动、内存、扫描、滚动数据 | 基准硬件 | NOT STARTED |

### 验证 1 的当前证据

已完成：

- NuGet 上 `Avalonia` / `Avalonia.Desktop` / `Avalonia.Headless` 最新稳定版均为 **12.1.4**；GitHub Release 发布于 2026-10-09。
- 新增 `spikes/Phase0/AvaloniaProbe`，目标框架 `net10.0`，引用 Avalonia 12.1.4。
- Windows：Debug 构建通过；GUI 进程启动后存活；`--smoke` 无头启动输出 `Avalonia headless startup: OK`。
- Windows 交叉发布 `linux-x64` 成功。
- 已添加手动 CI 工作流 `phase0-linux-smoke.yml`，在 Ubuntu runner 上执行无头启动。

未完成：

- 未在 Linux 桌面上验证 fcitx5/ibus 中文输入。
- Avalonia 上游存在未关闭的 Linux X11 DBus IME 问题：新窗口/对话框首次聚焦时 IME 可能不激活，切换窗口后恢复（AvaloniaUI/Avalonia#22315，2026-09-26 创建，当前 open）。
- 因此验证 1 只能记为 `PARTIAL`，Gate 0 不能通过。

验收补充：最低门槛是“fcitx5 或 ibus 至少一个可中文输入”；但登录/设置等**首次打开的对话框**如果必须切换窗口才能输入，应记为 P0 体验缺陷并纳入范围评审。

## 3. 环境缺口

| 需求 | 现状 | 影响 |
|---|---|---|
| Ubuntu 22.04/24.04 桌面 | 当前主机为 Windows，无 WSL/Docker | 1、2、3、4、6、8、9 无法在本机验证 |
| fcitx5 + 中文输入法、ibus 备选 | 未准备 | 验证 1 |
| GNOME X11 + Wayland 两会话 | 未准备 | 验证 1、6 |
| Java 8/17/21 矩阵 | 未准备 | 验证 4、9 |
| 4 核 / 8GB / SATA SSD 基准机 | 未固化 | 验证 7、10 |
| 200 Mod 测试库 | 未准备 | 验证 7 |
| 微软 OAuth 测试应用/账号 | 未准备 | 验证 2 |
| Windows 旧版数据样本 | 未准备 | Phase 1/2 兼容回归 |

## 4. 本轮已完成的启动工作

- 固化估算口径、WBS、日历和 Gate 规则（`docs/cross-platform-v2/`）。
- 完成当前代码基线审计：Core 46,361 行、WPF 引用 67 个文件、现有构建 0 error / 5,382 warnings。
- 完成测试基线：209 passed / 47 failed / 256 total；失败集中在 SQLite 文件锁、静态初始化和测试隔离，需在 Phase 1 前分类（[证据](evidence/2026-10-31-test-baseline.md)）。
- 建立 Avalonia 12.1.4 探针工程和 Linux 无头 CI 入口。
- 确认当前代码库没有目标平台接口、没有 Avalonia 依赖、Linux 运行时验证为空。

本轮**没有修改生产代码**，符合“Gate 0 前不进入 Phase 1”的约束。

## 5. 下一步（按优先级）

1. **准备 Ubuntu 22.04 桌面环境**（实机/VM/云主机），安装 .NET 10 SDK、fcitx5、ibus、GNOME X11 + Wayland。
2. 在 Linux 上运行 `spikes/Phase0/AvaloniaProbe`，分别记录：
   - 主窗口 TextBox 中文输入；
   - 首次打开的对话框 TextBox 中文输入；
   - 切换窗口后是否恢复；
   - `AVALONIA_IM_MODULE=fcitx` / `ibus` 差异；
   - X11 与 Wayland 差异。
3. 冻结 P0 页面清单和 `PCL.Core` / `PCL.UI` / `PCL.Platform.*` 边界，回填 WBS。
4. 启动验证 5（下载服务）和验证 7/10（性能基准）的 Windows 侧工作，这两项不依赖 Linux。
5. 准备微软 OAuth 测试应用与旧版数据样本。
6. 每周更新本执行板；Gate 0 评审时附全部原始日志、截图和命令。

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
| 47 个测试失败污染 QA 基线 | 高 | 中 | Phase 0 内完成分类；Phase 1 前修复阻塞类，其余隔离/标记 | 分类中 |
| 无 Linux 桌面环境导致 Gate 0 停滞 | 高 | 高 | 项目方提供 Ubuntu 云桌面/VM；不接受 WSLg/Docker 作为 IME/标题栏 Gate 证据 | 阻塞中 |
| 自定义标题栏在 Wayland 不可用 | 中 | 中 | Phase 0 实测；失败则按计划降级为系统装饰并更新 P0 验收 | 待验证 |
| AppImage 无 FUSE 环境不可运行 | 中 | 中 | Phase 0 实测 `--appimage-extract-and-run`；准备 libfuse2 文档 | 待验证 |
| `Avalonia.Diagnostics` 无 12.x 版本 | 高 | 低 | 不依赖该包；诊断使用日志/无头测试/其他 12.x 工具 | 已登记 |

风险登记每次 Gate 评审更新；新增高风险项必须同步到 WBS/日历影响评估。
