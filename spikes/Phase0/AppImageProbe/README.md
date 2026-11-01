# Phase 0 AppImage Probe

This packages the Avalonia IME probe as a Type 2 AppImage to exercise the AppImage runtime, FUSE launch, and `--appimage-extract-and-run` path. It is not the PCL launcher and does not satisfy the final product AppImage gate.

## Build on Ubuntu

Requirements: an `linux-x64` self-contained publish of `AvaloniaProbe`, `appimagetool`, and a writable output directory.

```bash
dotnet publish spikes/Phase0/AvaloniaProbe/AvaloniaProbe.csproj \
  -c Release -r linux-x64 --self-contained true \
  -o artifact/phase0/avalonia-probe-linux-x64

./spikes/Phase0/AppImageProbe/build-appimage.sh \
  artifact/phase0/avalonia-probe-linux-x64 \
  artifact/phase0/PCL-Phase0-AvaloniaProbe-x86_64.AppImage
```

`appimagetool` is intentionally not vendored. Install it from a trusted source and verify its publisher/checksum before use.

On 2026-11-01, this probe was packaged on Ubuntu 22.04.5 with AppImageKit `appimagetool` continuous build commit `5735cc5`. Both FUSE-backed `--smoke` and explicit `--appimage-extract-and-run --smoke` passed. The host had `/dev/fuse`; no-FUSE hardware/VM and GUI-window launch remain for manual verification. See [`2026-11-01-appimage-probe-ubuntu.md`](../../../docs/cross-platform-v2/phase-0/evidence/2026-11-01-appimage-probe-ubuntu.md).

## Ubuntu smoke tests

Run both commands from a GNOME desktop session:

```bash
chmod +x artifact/phase0/PCL-Phase0-AvaloniaProbe-x86_64.AppImage
./artifact/phase0/PCL-Phase0-AvaloniaProbe-x86_64.AppImage
./artifact/phase0/PCL-Phase0-AvaloniaProbe-x86_64.AppImage --appimage-extract-and-run
```

Both should open the Avalonia probe window. The first uses the normal AppImage runtime; the second explicitly exercises the runtime's extract-and-run fallback. This does not prove behavior on a machine where `/dev/fuse` is absent, so record the actual FUSE state and retain that limitation in the evidence.

For the already packaged VM build, run these from the GNOME desktop terminal:

```bash
cd /home/user/phase0-appimage-probe-20261101
chmod +x PCL-Phase0-AvaloniaProbe-x86_64.AppImage
./PCL-Phase0-AvaloniaProbe-x86_64.AppImage
./PCL-Phase0-AvaloniaProbe-x86_64.AppImage --appimage-extract-and-run
```
