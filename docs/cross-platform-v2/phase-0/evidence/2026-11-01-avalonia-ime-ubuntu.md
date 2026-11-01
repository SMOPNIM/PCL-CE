# 验证 1 补充证据：Ubuntu 桌面 IME

> 日期：2026-11-01
> 探针提交：`ac5386a7`
> 环境：Ubuntu 22.04.5 LTS / GNOME Wayland / fcitx5 5.0.14
> 结论：**PASS（已测组合）**。主窗口和首次打开的对话框均可直接输入中文。

## 执行方式

在 Ubuntu 桌面终端启动已交叉发布的探针：

```bash
~/phase0-avprobe-test-20261101/PCL.Phase0.AvaloniaProbe
```

该二进制由提交 `ac5386a7` 的 `spikes/Phase0/AvaloniaProbe` 工程以 `linux-x64`、self-contained 方式发布，并通过 SSH 传输到测试机。测试用户在虚拟机桌面进行交互测试。

## 结果

| 场景 | 结果 |
|---|---|
| 主窗口输入框自动聚焦 | 正常 |
| 主窗口 fcitx5 中文输入 | 正常 |
| 首次打开对话框，不切换窗口直接输入中文 | 正常 |

结果由测试用户在桌面交互确认；本次未保存截图或桌面端原始日志。

## 覆盖范围与结论

- 已覆盖：Ubuntu 22.04.5、GNOME Wayland、fcitx5 5.0.14。
- 未覆盖：GNOME X11、ibus。
- Avalonia 12.1.4 稳定版、Windows 构建、无头启动和 Linux 交叉发布的证据见 [2026-10-31 版本与启动记录](2026-10-31-avalonia-12.1.4.md)。
- 本次测试满足验证 1 的最低通过标准，也通过了已测组合下首次打开对话框的直接输入体验标准。未测试组合及上游 X11 风险仍需跟踪。
- 验证 1 标记 `PASS`；Phase 0 Gate 0 仍未通过，其他强制验证项尚待完成。
