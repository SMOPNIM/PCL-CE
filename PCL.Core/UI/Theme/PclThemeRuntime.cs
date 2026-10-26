using System;
using System.Collections.Generic;
using PCL.Core.App;

namespace PCL.Core.UI.Theme;

/// <summary>
/// 经典主题运行时：负责调色板缓存、主题 14 的实时参数以及主题 12 的随机游走。
/// <para>
/// <see cref="RefreshPalette"/> 只会在“切换到 12 号主题”时推进一步随机游走，
/// 重复刷新不会继续推进；因此 <c>ThemeManager</c> 可以安全地反复读取当前调色板。
/// </para>
/// </summary>
public static class PclThemeRuntime
{
    private static readonly object SyncRoot = new();
    private static readonly Dictionary<int, PclThemePalette> Cache = new();

    private static PclThemePalette? _current;
    private static int _lastThemeId = -1;
    private static PclThemeCustomSettings _lastCustom;

    private static bool _theme12Seeded;
    private static int _theme12Hue;
    private static int _theme12Step;

    /// <summary>
    /// 当前调色板。首次访问时按当前配置构建。
    /// </summary>
    public static PclThemePalette Current => _current ?? RefreshPalette();

    /// <summary>
    /// 当前 12 号主题的色相，主要用于诊断与测试。
    /// </summary>
    public static int Theme12Hue => _theme12Hue;

    /// <summary>
    /// 当前 12 号主题每一步的固定偏移，只会是 +4 或 -4。
    /// </summary>
    public static int Theme12Step => _theme12Step;

    /// <summary>
    /// 按当前配置刷新调色板。
    /// </summary>
    public static PclThemePalette RefreshPalette()
    {
        lock (SyncRoot)
        {
            var theme = Config.Preference.Theme;
            var requestedId = theme.ThemeSelected;
            var definition = PclThemeCatalog.GetById(requestedId) ?? PclThemeCatalog.GetById(0)!;

            var hue = definition.Hue;
            PclThemeCustomSettings custom = default;

            switch (definition.Id)
            {
                case 12:
                    if (!_theme12Seeded)
                    {
                        _theme12Hue = Random.Shared.Next(0, 360);
                        _theme12Step = (Random.Shared.Next(0, 2) * 2 - 1) * 4;
                        _theme12Seeded = true;
                    }

                    if (_lastThemeId != 12)
                    {
                        _theme12Hue = (_theme12Hue + _theme12Step) % 360;
                        if (_theme12Hue < 0) _theme12Hue += 360;
                        Cache.Remove(12);
                    }

                    hue = _theme12Hue;
                    break;

                case 14:
                    custom = new PclThemeCustomSettings(
                        Hue: theme.WindowHue,
                        Saturation: theme.WindowSat,
                        LightAdjust: theme.WindowLight - 20,
                        TitlebarDelta: theme.WindowDelta - 90);
                    if (_lastCustom != custom)
                    {
                        _lastCustom = custom;
                        Cache.Remove(14);
                    }

                    hue = custom.Hue;
                    break;
            }

            if (!Cache.TryGetValue(definition.Id, out var palette))
            {
                palette = PclThemePalette.Create(definition, hue, custom);
                Cache[definition.Id] = palette;
            }

            _lastThemeId = definition.Id;
            _current = palette;
            return palette;
        }
    }

    /// <summary>
    /// 清除指定主题的缓存，下一次读取时会重新计算。
    /// </summary>
    public static void Invalidate(int? themeId = null)
    {
        lock (SyncRoot)
        {
            if (themeId is { } id) Cache.Remove(id);
            else Cache.Clear();
            _current = null;
        }
    }

    /// <summary>
    /// 标记当前已离开经典主题，使下次重新进入时 12 号主题继续其随机游走。
    /// </summary>
    internal static void ResetCurrentThemeTracking()
    {
        lock (SyncRoot)
        {
            _lastThemeId = -1;
        }
    }

    /// <summary>
    /// 重置全部运行时状态，主要用于测试。
    /// </summary>
    internal static void Reset()
    {
        lock (SyncRoot)
        {
            Cache.Clear();
            _current = null;
            _lastThemeId = -1;
            _lastCustom = default;
            _theme12Seeded = false;
            _theme12Hue = 0;
            _theme12Step = 0;
        }
    }
}
