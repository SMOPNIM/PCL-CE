using System;

namespace PCL.Core.UI.Theme;

/// <summary>
/// 与 PCL 原版 <c>ModBase.MyColor</c> 行为一致的 RGB 颜色值。
/// 各通道使用 0~255 的双精度浮点数，以保留原版配色计算中的小数。
/// </summary>
public readonly struct PclColor : IEquatable<PclColor>
{
    /// <summary>
    /// 原版 <c>FromHSL2</c> 使用的亮度分区修正表。
    /// </summary>
    private static readonly double[] LightnessAnchors =
    [
        +0.10d, -0.06d, -0.30d, -0.19d, -0.15d, -0.24d, -0.32d, -0.09d,
        +0.18d, +0.05d, -0.12d, -0.02d, +0.10d, -0.06d
    ];

    public PclColor(double red, double green, double blue, double alpha = 255d)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    /// <summary>
    /// 红色通道 (0~255)。
    /// </summary>
    public double Red { get; }

    /// <summary>
    /// 绿色通道 (0~255)。
    /// </summary>
    public double Green { get; }

    /// <summary>
    /// 蓝色通道 (0~255)。
    /// </summary>
    public double Blue { get; }

    /// <summary>
    /// 透明度通道 (0~255)。
    /// </summary>
    public double Alpha { get; }

    public byte RedByte => ToByte(Red);

    public byte GreenByte => ToByte(Green);

    public byte BlueByte => ToByte(Blue);

    public byte AlphaByte => ToByte(Alpha);

    public static PclColor FromRgb(double red, double green, double blue, double alpha = 255d) =>
        new(red, green, blue, alpha);

    /// <summary>
    /// 原版 <c>ModBase.MyColor.FromHSL2</c> 的精确移植。
    /// 该方法会先根据色相与饱和度重映射明度，再走标准 HSL 转换。
    /// </summary>
    public static PclColor FromHsl2(double hue, double saturation, double lightness)
    {
        if (saturation == 0d)
        {
            var gray = lightness * 2.55d;
            return new PclColor(gray, gray, gray);
        }

        hue = (hue + 3600000d) % 360d;
        var segment = hue / 30d;
        var index = (int)Math.Round(Math.Floor(segment)); // 亮度片区编号
        var threshold = 50d -
                        ((1d - segment + index) * LightnessAnchors[index] +
                         (segment - index) * LightnessAnchors[index + 1]) * saturation;
        var mappedLightness = (lightness < threshold
            ? lightness / threshold
            : 1d + (lightness - threshold) / (100d - threshold)) * 50d;

        return FromHsl(hue, saturation, mappedLightness);
    }

    /// <summary>
    /// 标准 HSL 转换，保留原版 <c>FromHSL</c> 的实现细节。
    /// </summary>
    public static PclColor FromHsl(double hue, double saturation, double lightness)
    {
        if (saturation == 0d)
        {
            var gray = lightness * 2.55d;
            return new PclColor(gray, gray, gray);
        }

        var h = hue / 360d;
        var s = saturation / 100d;
        var l = lightness / 100d;
        s = l < 0.5d ? s * l + l : s * (1d - l) + l;
        l = 2d * l - s;
        return new PclColor(
            255d * Hue2Rgb(l, s, h + 1d / 3d),
            255d * Hue2Rgb(l, s, h),
            255d * Hue2Rgb(l, s, h - 1d / 3d));
    }

    public PclColor WithAlpha(double alpha) => new(Red, Green, Blue, alpha);

    /// <summary>
    /// 将颜色转换到带有资源的 <see cref="CatColorResource"/>。
    /// 使用精确的 RGB 值经过色彩空间转换，避免 scRGB 直接赋值造成偏色。
    /// </summary>
    public CatColorResource ToCatColor(string suffix, bool applyToColor = true, bool applyToBrush = true) =>
        LabColor.FromRgb(RedByte, GreenByte, BlueByte, AlphaByte / 255d)
            .ToCatColor(suffix, applyToColor, applyToBrush);

    public System.Windows.Media.Color ToWpfColor() =>
        System.Windows.Media.Color.FromArgb(AlphaByte, RedByte, GreenByte, BlueByte);

    private static double Hue2Rgb(double v1, double v2, double hue)
    {
        if (hue < 0d) hue += 1d;
        if (hue > 1d) hue -= 1d;
        if (hue < 0.16667d) return v1 + (v2 - v1) * 6d * hue;
        if (hue < 0.5d) return v2;
        if (hue < 0.66667d) return v1 + (v2 - v1) * (4d - hue * 6d);
        return v1;
    }

    private static byte ToByte(double value) =>
        (byte)Math.Round(Math.Clamp(value, 0d, 255d));

    public bool Equals(PclColor other) =>
        Red.Equals(other.Red) && Green.Equals(other.Green) && Blue.Equals(other.Blue) && Alpha.Equals(other.Alpha);

    public override bool Equals(object? obj) => obj is PclColor other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

    public static bool operator ==(PclColor left, PclColor right) => left.Equals(right);

    public static bool operator !=(PclColor left, PclColor right) => !left.Equals(right);

    public override string ToString() => $"#{AlphaByte:X2}{RedByte:X2}{GreenByte:X2}{BlueByte:X2}";
}
