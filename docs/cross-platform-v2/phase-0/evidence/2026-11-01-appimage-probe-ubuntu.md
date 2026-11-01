# 验证 8 补充证据：Avalonia AppImage 探针

> 日期：2026-11-01
> 环境：Ubuntu 22.04.5 LTS / x86_64 / glibc 2.35
> 结论：**AppImage 探针 PARTIAL。** 正常 FUSE 与显式 extract-and-run 的无头 smoke 均通过；无 FUSE 和 PCL 产物尚未验证。

## 产物

使用 `spikes/Phase0/AvaloniaProbe` 的 Linux x64 self-contained publish 打包：

```text
/home/user/phase0-appimage-probe-20261101/PCL-Phase0-AvaloniaProbe-x86_64.AppImage
SHA-256: 7bbf8f8f9d70f3efbd4dd24df379b32047b9101e7c8b403545e5fd1c63214415
```

使用 AppImageKit `appimagetool` continuous build，构建标识 `5735cc5`（2023-03-08），下载来源：
`https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage`

本次下载工具的本地 SHA-256：

```text
b90f4a8b18967545fda78a445b27680a1642f1ef9488ced28b65398f2be7add2
```

该 hash 是下载后计算的记录值，不是独立签名/上游校验和验证。

## 执行结果

```text
glibc=ldd (Ubuntu GLIBC 2.35-0ubuntu3.8) 2.35
fuse=/dev/fuse available
```

通过 SSH 在有 FUSE 的主机上执行，`--smoke` 避免尝试打开 GUI：

```bash
./PCL-Phase0-AvaloniaProbe-x86_64.AppImage --smoke
./PCL-Phase0-AvaloniaProbe-x86_64.AppImage --appimage-extract-and-run --smoke
```

两条命令均输出：

```text
Avalonia headless startup: OK
```

用户确认 GNOME 桌面下普通启动和 `--appimage-extract-and-run` 两条路径均成功打开窗口。

## 限制

- 这是 Avalonia IME 探针的 AppImage，不是 PCL 启动器 AppImage。
- 当前 VM 存在 `/dev/fuse`；第二条命令显式覆盖 extract-and-run 运行路径，但不等同于在 FUSE 设备缺失的环境实测。
- 无 FUSE 主机尚未实测；两条 GUI 路径的成功结果由用户确认。
- 因此验证 8 标记为 `PARTIAL`；Phase 0 Gate 0 仍未通过。
