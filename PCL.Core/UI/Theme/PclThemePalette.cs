using System;
using System.Collections.Generic;

namespace PCL.Core.UI.Theme;

/// <summary>
/// 渐变色标。
/// </summary>
public readonly record struct PclColorStop(double Offset, PclColor Color);

/// <summary>
/// 自定义经典主题（14 号）的输入参数。
/// </summary>
public readonly record struct PclThemeCustomSettings(
    int Hue = 180,
    int Saturation = 80,
    int LightAdjust = 0,
    int TitlebarDelta = 0);

/// <summary>
/// 经典主题的完整调色板。
/// </summary>
public sealed class PclThemePalette
{
    private const double Gray4 = 166d;

    public required int ThemeId { get; init; }

    public required PclColor Color1 { get; init; }
    public required PclColor Color2 { get; init; }
    public required PclColor Color3 { get; init; }
    public required PclColor Color4 { get; init; }
    public required PclColor Color5 { get; init; }
    public required PclColor Color6 { get; init; }
    public required PclColor Color7 { get; init; }
    public required PclColor Color8 { get; init; }
    public required PclColor Background0 { get; init; }
    public required PclColor Background1 { get; init; }
    public required PclColor SemiTransparent { get; init; }

    public required IReadOnlyList<PclColorStop> TitlebarStops { get; init; }
    public required IReadOnlyList<PclColorStop> BackgroundStops { get; init; }

    public required bool HasBanner { get; init; }
    public required double BannerOpacity { get; init; }

    /// <summary>
    /// 根据主题定义构建调色板。
    /// </summary>
    /// <param name="definition">主题定义。</param>
    /// <param name="hue">实际使用的色相，用于主题 12 的随机游走与主题 14 的自定义色相。</param>
    /// <param name="custom">主题 14 的自定义参数。</param>
    public static PclThemePalette Create(
        PclThemeDefinition definition,
        int hue,
        PclThemeCustomSettings custom = default)
    {
        var saturation = definition.Id == 14 ? custom.Saturation : definition.Saturation;
        var lightAdjust = definition.Id == 14 ? custom.LightAdjust : definition.LightAdjust;

        PclColor Hsl(double h, double s, double l) => PclColor.FromHsl2(h, s, l);

        var color1 = Hsl(hue, saturation * 0.2d, 25d + lightAdjust * 0.3d);
        var color2 = Hsl(hue, saturation, 45d + lightAdjust);
        var color3 = Hsl(hue, saturation, 55d + lightAdjust);
        var color4 = Hsl(hue, saturation, 65d + lightAdjust);
        var color5 = Hsl(hue, saturation, 80d + lightAdjust * 0.4d);
        var color6 = Hsl(hue, saturation, 91d + lightAdjust * 0.1d);
        var color7 = Hsl(hue, saturation, 94d);
        var color8 = Hsl(hue, saturation, 97d);

        var background0 = new PclColor(
            color4.Red * 0.4d + color5.Red * 0.4d + Gray4 * 0.2d,
            color4.Green * 0.4d + color5.Green * 0.4d + Gray4 * 0.2d,
            color4.Blue * 0.4d + color5.Blue * 0.4d + Gray4 * 0.2d);

        var titlebarStops = CreateTitlebarStops(definition, hue, saturation, lightAdjust, custom);
        var backgroundStops = new PclColorStop[]
        {
            new(-0.1d, Hsl(hue - 20d, Math.Min(60d, saturation) * 0.5d, 80d)),
            new(0.4d, Hsl(hue, saturation * 0.9d, 90d)),
            new(1.1d, Hsl(hue + 20d, Math.Min(60d, saturation) * 0.5d, 80d))
        };

        return new PclThemePalette
        {
            ThemeId = definition.Id,
            Color1 = color1,
            Color2 = color2,
            Color3 = color3,
            Color4 = color4,
            Color5 = color5,
            Color6 = color6,
            Color7 = color7,
            Color8 = color8,
            Background0 = background0,
            Background1 = color7.WithAlpha(190d),
            SemiTransparent = color8.WithAlpha(1d),
            TitlebarStops = titlebarStops,
            BackgroundStops = backgroundStops,
            HasBanner = definition.HasBanner,
            BannerOpacity = definition.BannerOpacity
        };
    }

    private static IReadOnlyList<PclColorStop> CreateTitlebarStops(
        PclThemeDefinition definition,
        int hue,
        int saturation,
        int lightAdjust,
        PclThemeCustomSettings custom)
    {
        switch (definition.TitlebarMode)
        {
            case PclTitlebarMode.FiveStop:
                return
                [
                    new(0d, PclColor.FromHsl2(hue, saturation, 25d)),
                    new(0.5d, PclColor.FromHsl2(hue, saturation, 15d)),
                    new(1d, PclColor.FromHsl2(hue, saturation, 25d))
                ];

            case PclTitlebarMode.FourStop:
                return
                [
                    new(0d, PclColor.FromHsl2(hue - 21d, saturation, 53d + lightAdjust)),
                    new(0.33d, PclColor.FromHsl2(hue - 7d, saturation, 47d + lightAdjust)),
                    new(0.67d, PclColor.FromHsl2(hue + 7d, saturation, 47d + lightAdjust)),
                    new(1d, PclColor.FromHsl2(hue + 21d, saturation, 53d + lightAdjust))
                ];

            default:
            {
                int[] deltas;
                if (definition.Id == 14)
                {
                    deltas = [-custom.TitlebarDelta, 0, custom.TitlebarDelta];
                }
                else if (definition.TitlebarHueDeltas is { } array)
                {
                    deltas = array;
                }
                else
                {
                    deltas = [-definition.TitlebarHueDelta, 0, definition.TitlebarHueDelta];
                }

                var lightness = new[] { 48d + lightAdjust, 54d + lightAdjust, 48d + lightAdjust };
                var offsets = new[] { 0d, 0.5d, 1d };
                var stops = new PclColorStop[3];
                for (var i = 0; i < 3; i++)
                    stops[i] = new PclColorStop(offsets[i],
                        PclColor.FromHsl2(hue + deltas[i], saturation, lightness[i]));
                return stops;
            }
        }
    }

    /// <summary>
    /// 将调色板写入 WPF 资源字典。
    /// </summary>
    public void ApplyResources()
    {
        Color1.ToCatColor("1").Apply();
        Color2.ToCatColor("2").Apply();
        Color3.ToCatColor("3").Apply();
        Color4.ToCatColor("4").Apply();
        Color5.ToCatColor("5").Apply();
        Color6.ToCatColor("6").Apply();
        Color7.ToCatColor("7").Apply();
        Color8.ToCatColor("8").Apply();
        Background0.ToCatColor("Bg0").Apply();
        Background1.ToCatColor("Bg1").Apply();
        SemiTransparent.ToCatColor("SemiTransparent", applyToColor: false).Apply();
    }
}
