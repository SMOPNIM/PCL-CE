#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 2 ]]; then
    echo "Usage: $0 <linux-publish-directory> <output-appimage>" >&2
    exit 2
fi

publish_dir="$(realpath "$1")"
output_file="$(realpath -m "$2")"
probe_binary="$publish_dir/PCL.Phase0.AvaloniaProbe"
appdir="$(mktemp -d "${TMPDIR:-/tmp}/pcl-phase0-appimage.XXXXXX")"
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
mkdir -p "$(dirname "$output_file")"

cleanup() {
    rm -rf -- "$appdir"
}
trap cleanup EXIT

if [[ ! -x "$probe_binary" ]]; then
    echo "Linux probe executable not found or not executable: $probe_binary" >&2
    exit 1
fi
if ! command -v appimagetool >/dev/null 2>&1; then
    echo "appimagetool is required and must be available on PATH." >&2
    exit 1
fi

mkdir -p "$appdir/usr/bin" "$appdir/usr/share/icons/hicolor/scalable/apps"
cp -a "$publish_dir/." "$appdir/usr/bin/"
cp "$script_dir/AppRun" "$appdir/AppRun"
cp "$script_dir/PCL.Phase0.AvaloniaProbe.desktop" "$appdir/"
cp "$script_dir/appicon.svg" "$appdir/appicon.svg"
cp "$script_dir/appicon.svg" "$appdir/usr/share/icons/hicolor/scalable/apps/appicon.svg"
chmod 755 "$appdir/AppRun" "$appdir/usr/bin/PCL.Phase0.AvaloniaProbe"

appimagetool "$appdir" "$output_file"
printf 'Created AppImage: %s\n' "$output_file"
