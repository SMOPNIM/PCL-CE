namespace PCL.Core.UI.Theme;

/// <summary>
/// 经典主题标题栏渐变模式。
/// </summary>
public enum PclTitlebarMode
{
    /// <summary>
    /// 标准三段渐变。
    /// </summary>
    Standard,

    /// <summary>
    /// 四段渐变（滑稽彩）。
    /// </summary>
    FourStop,

    /// <summary>
    /// 五段渐变（玄素黑）。
    /// </summary>
    FiveStop
}

/// <summary>
/// 经典主题解锁方式，供后续阶段实现解锁逻辑时使用。
/// </summary>
public enum PclThemeUnlockKind
{
    Always,
    ClickOverlay,
    Launches,
    LegacyRitual,
    Donation,
    BugReport,
    MicrosoftLogin,
    Puzzle,
    AprilFools,
    Jrrp,
    FiveHidden,
    ConfigOnly
}

/// <summary>
/// PCL 原版经典主题的静态定义。
/// </summary>
public sealed record PclThemeDefinition
{
    public required int Id { get; init; }

    public required string NameKey { get; init; }

    public string? TooltipKey { get; init; }

    /// <summary>
    /// 选择器行号，42 号主题为 <see langword="null"/>。
    /// </summary>
    public int? SelectorRow { get; init; }

    /// <summary>
    /// 选择器列号（从 1 开始，0 列用于行标签），42 号主题为 <see langword="null"/>。
    /// </summary>
    public int? SelectorColumn { get; init; }

    /// <summary>
    /// 是否属于隐藏主题。
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// 是否计入“已解锁隐藏主题数量”。
    /// </summary>
    public bool CountedAsHidden { get; init; }

    /// <summary>
    /// 基础色相。
    /// </summary>
    public int Hue { get; init; }

    /// <summary>
    /// 基础饱和度。
    /// </summary>
    public int Saturation { get; init; }

    /// <summary>
    /// 明度调整量。
    /// </summary>
    public int LightAdjust { get; init; }

    /// <summary>
    /// 标题栏色相偏移，标量形式。
    /// </summary>
    public int TitlebarHueDelta { get; init; }

    /// <summary>
    /// 标题栏色相偏移，数组形式。存在时优先于 <see cref="TitlebarHueDelta"/>。
    /// </summary>
    public int[]? TitlebarHueDeltas { get; init; }

    public required PclTitlebarMode TitlebarMode { get; init; }

    public required PclThemeUnlockKind UnlockKind { get; init; }

    /// <summary>
    /// 解锁条件的原始描述，仅用于后续阶段展示，不参与当前配色逻辑。
    /// </summary>
    public string? UnlockRequirement { get; init; }

    /// <summary>
    /// 是否带有标题栏背景图。
    /// </summary>
    public bool HasBanner => Id is >= 5 and not 14;

    /// <summary>
    /// 标题栏背景图透明度。
    /// </summary>
    public double BannerOpacity => Id == 13 ? 0.25d : 0.5d;

    /// <summary>
    /// 主题 12 与 14 的配色由运行时决定。
    /// </summary>
    public bool IsDynamic => Id is 12 or 14;
}
