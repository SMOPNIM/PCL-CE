using System.Collections.Generic;
using System.Linq;

namespace PCL.Core.UI.Theme;

/// <summary>
/// PCL 原版经典主题目录。
/// 选择器布局与 2.2.3 保持一致：三行五列，42 号主题没有单选按钮。
/// </summary>
public static class PclThemeCatalog
{
    /// <summary>
    /// 全部经典主题定义，按 ID 排列。
    /// </summary>
    public static IReadOnlyList<PclThemeDefinition> All { get; } =
    [
        new()
        {
            Id = 0, NameKey = "Setup.Ui.ClassicTheme.Name.0", SelectorRow = 0, SelectorColumn = 1,
            Hue = 210, Saturation = 85, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Always, UnlockRequirement = "allowlist"
        },
        new()
        {
            Id = 1, NameKey = "Setup.Ui.ClassicTheme.Name.1", SelectorRow = 0, SelectorColumn = 2,
            Hue = 180, Saturation = 50, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Always, UnlockRequirement = "allowlist"
        },
        new()
        {
            Id = 2, NameKey = "Setup.Ui.ClassicTheme.Name.2", SelectorRow = 0, SelectorColumn = 3,
            Hue = 130, Saturation = 40, TitlebarHueDeltas = [5, -5, 5],
            TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Always, UnlockRequirement = "allowlist"
        },
        new()
        {
            Id = 3, NameKey = "Setup.Ui.ClassicTheme.Name.3", SelectorRow = 0, SelectorColumn = 4,
            Hue = 75, Saturation = 50, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Always, UnlockRequirement = "allowlist"
        },
        new()
        {
            Id = 4, NameKey = "Setup.Ui.ClassicTheme.Name.4", SelectorRow = 0, SelectorColumn = 5,
            Hue = 35, Saturation = 30, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Always, UnlockRequirement = "allowlist"
        },
        new()
        {
            Id = 5, NameKey = "Setup.Ui.ClassicTheme.Name.5",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.5",
            SelectorRow = 1, SelectorColumn = 1, IsHidden = true, CountedAsHidden = true,
            Hue = 225, Saturation = 10, LightAdjust = -20, TitlebarMode = PclTitlebarMode.FiveStop,
            UnlockKind = PclThemeUnlockKind.ClickOverlay, UnlockRequirement = "rapid click overlay"
        },
        new()
        {
            Id = 6, NameKey = "Setup.Ui.ClassicTheme.Name.6",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.6",
            SelectorRow = 1, SelectorColumn = 3, IsHidden = true, CountedAsHidden = true,
            Hue = 345, Saturation = 70, LightAdjust = 15, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Launches, UnlockRequirement = "99 launches"
        },
        new()
        {
            Id = 7, NameKey = "Setup.Ui.ClassicTheme.Name.7",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.7",
            SelectorRow = 1, SelectorColumn = 4, IsHidden = true, CountedAsHidden = true,
            Hue = 285, Saturation = 50, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.LegacyRitual, UnlockRequirement = "legacy dev 0.4.4 ritual"
        },
        new()
        {
            Id = 8, NameKey = "Setup.Ui.ClassicTheme.Name.8",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.8",
            SelectorRow = 2, SelectorColumn = 1, IsHidden = true, CountedAsHidden = true,
            Hue = 50, Saturation = 65, LightAdjust = 10, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Donation, UnlockRequirement = "rank >= 23 or gold code"
        },
        new()
        {
            Id = 9, NameKey = "Setup.Ui.ClassicTheme.Name.9",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.9",
            SelectorRow = 2, SelectorColumn = 2, IsHidden = true, CountedAsHidden = true,
            Hue = 25, Saturation = 75, LightAdjust = 5, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.BugReport, UnlockRequirement = "qualifying bug report or pull request"
        },
        new()
        {
            Id = 10, NameKey = "Setup.Ui.ClassicTheme.Name.10",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.10",
            SelectorRow = 2, SelectorColumn = 3, IsHidden = true, CountedAsHidden = true,
            Hue = 5, Saturation = 70, LightAdjust = -5, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.MicrosoftLogin, UnlockRequirement = "successful Microsoft login"
        },
        new()
        {
            Id = 11, NameKey = "Setup.Ui.ClassicTheme.Name.11",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.11",
            SelectorRow = 2, SelectorColumn = 4, IsHidden = true, CountedAsHidden = true,
            Hue = 230, Saturation = 60, LightAdjust = -5, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Puzzle, UnlockRequirement = "3 hidden themes and puzzle code"
        },
        new()
        {
            Id = 12, NameKey = "Setup.Ui.ClassicTheme.Name.12",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.12",
            SelectorRow = 1, SelectorColumn = 2, IsHidden = true, CountedAsHidden = true,
            Hue = 0, Saturation = 45, TitlebarMode = PclTitlebarMode.FourStop,
            UnlockKind = PclThemeUnlockKind.AprilFools, UnlockRequirement = "launch on April 1"
        },
        new()
        {
            Id = 13, NameKey = "Setup.Ui.ClassicTheme.Name.13",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.13",
            SelectorRow = 1, SelectorColumn = 5, IsHidden = true, CountedAsHidden = true,
            Hue = 290, Saturation = 50, TitlebarHueDeltas = [-30, 10, 50],
            TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.Jrrp, UnlockRequirement = "Jrrp == 100"
        },
        new()
        {
            Id = 14, NameKey = "Setup.Ui.ClassicTheme.Name.14",
            TooltipKey = "Setup.Ui.ClassicTheme.Tooltip.14",
            SelectorRow = 2, SelectorColumn = 5, IsHidden = true, CountedAsHidden = true,
            Hue = 180, Saturation = 80, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.FiveHidden, UnlockRequirement = "5 hidden themes"
        },
        new()
        {
            Id = 42, NameKey = "Setup.Ui.ClassicTheme.Name.42",
            Hue = 225, Saturation = 60, LightAdjust = -15, TitlebarMode = PclTitlebarMode.Standard,
            UnlockKind = PclThemeUnlockKind.ConfigOnly, UnlockRequirement = "Setup.ini only"
        }
    ];

    /// <summary>
    /// 在经典主题选择器中显示的主题。
    /// </summary>
    public static IReadOnlyList<PclThemeDefinition> Selectable { get; } =
        All.Where(definition => definition.SelectorRow is not null).ToArray();

    public static PclThemeDefinition? GetById(int id) =>
        All.FirstOrDefault(definition => definition.Id == id);
}
