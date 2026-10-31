# Avalonia Phase 0 探针

用于 Phase 0 验证 1（Avalonia 版本与 Linux IME）。**不是生产工程，不加入解决方案，不进入发布包。**

## 目的

1. 确认 Avalonia 12.1.4 + .NET 10 在当前平台可构建、可启动。
2. 在 Linux 桌面验证 fcitx5 / ibus 中文输入。
3. 复现“新对话框首次聚焦时 IME 不激活”的上游问题（AvaloniaUI/Avalonia#22315）。
4. 为后续自定义标题栏、主题、性能探针提供最小宿主。

## 结构

- `Program.cs`：桌面启动 + `--smoke` 无头启动。
- `MainWindow.axaml`：主窗口中文输入框 + 对话框探针按钮。
- `MainWindow.axaml.cs`：打开一个自动聚焦 TextBox 的对话框。

## Windows

```powershell
dotnet build spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Debug

# 无头启动，适用于 CI
dotnet run --project spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Debug -- --smoke

# 有界面启动
dotnet run --project spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Debug
```

预期：`--smoke` 输出 `Avalonia headless startup: OK` 并以 0 退出。

## Linux 桌面

先安装 .NET 10 SDK、CJK 字体和至少一个输入法：

```bash
# 示例，按发行版调整
sudo apt install fonts-noto-cjk fcitx5 fcitx5-chinese-addons ibus
dotnet --info
```

构建并启动：

```bash
dotnet build spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Release
dotnet run --project spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Release --no-build
```

### 验证步骤

记录环境：

```bash
echo "session=$XDG_SESSION_TYPE desktop=$XDG_CURRENT_DESKTOP"
fcitx5 --version || true
ibus version || true
dotnet --info | head -n 20
```

1. **主窗口**：在“主窗口输入框”输入中文。通过标准：候选框出现，文字正确上屏。
2. **对话框首次聚焦**：点击“打开对话框 IME 探针”，不要切换窗口，直接在输入框输入中文。通过标准：候选框出现，文字正确上屏。
3. 如果第 2 步失败，切到其他窗口再切回，再输入一次，记录是否恢复。
4. 分别用 fcitx5 和 ibus 重复：

   ```bash
   AVALONIA_IM_MODULE=fcitx dotnet run --project spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Release
   AVALONIA_IM_MODULE=ibus  dotnet run --project spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj -c Release
   ```

5. 在 GNOME X11 和 Wayland 两个会话各做一遍。
6. 截图保存到 `artifact/phase0/ime/`，把结论写回 `docs/cross-platform-v2/phase-0/README.md` 和证据文件。

### 判定

- **PASS（最低门槛）**：至少一个输入法在至少一个会话可中文输入。
- **PASS（P0 体验门槛）**：主窗口与首次打开的对话框都可直接输入，不需要切换窗口。
- **PARTIAL / FAIL**：只有主窗口可用，或对话框必须切换窗口才能输入。记录为范围风险，提交 Gate 0 评审。

## 交叉发布 linux-x64

```powershell
dotnet publish spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj `
  -c Release -r linux-x64 --self-contained false `
  -o artifact/phase0/avalonia-probe-linux-x64
```

产物不含 .NET 运行时，目标机需安装 .NET 10 Desktop/Runtime 依赖。发布目录仅用于验证，不提交仓库。

## 已知上游风险

- [AvaloniaUI/Avalonia#22315](https://github.com/AvaloniaUI/Avalonia/issues/22315)：Linux X11/XWayland 下，新窗口或对话框首次聚焦时 DBus IME 可能不激活；切换窗口后恢复。创建于 2026-09-26，当前 open。
- `Avalonia.Diagnostics` 没有 12.x 版本（NuGet 最新为 11.3.22），迁移时不要引用；需要诊断工具时另找 12.x 方案。
