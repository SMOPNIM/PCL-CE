using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.Core.App;
using PCL.Core.UI.Theme;

namespace PCL;

public static class ThemeManager
{
    private static bool _contextMenuHandlerRegistered;

    public static bool IsDarkMode => ThemeService.IsDarkMode;

    public static ResourceDictionary AppResources => System.Windows.Application.Current.Resources;

    public static ModBase.MyColor colorGray1 = new(AppResources["ColorObjectGray1"]);
    public static ModBase.MyColor colorGray4 = new(AppResources["ColorObjectGray4"]);
    public static ModBase.MyColor colorGray5 = new(AppResources["ColorObjectGray5"]);
    public static ModBase.MyColor colorSemiTransparent = new(AppResources["ColorBrushSemiTransparent"]);

    public static void ThemeRefresh(int newTheme = -1)
    {
        colorGray1 = new ModBase.MyColor(AppResources["ColorObjectGray1"]);
        colorGray4 = new ModBase.MyColor(AppResources["ColorObjectGray4"]);
        colorGray5 = new ModBase.MyColor(AppResources["ColorObjectGray5"]);
        colorSemiTransparent = new ModBase.MyColor(AppResources["ColorBrushSemiTransparent"]);
        ThemeRefreshMain();
    }

    public static void ThemeRefreshMain()
    {
        ModBase.RunInUi(() =>
        {
            if (!ModMain.frmMain.IsLoaded) return;
            RefreshTitlebar();
            RefreshBackground();
            RefreshAllContextMenuThemes();
        });
    }

    // 标题栏渐变与背景图
    private static void RefreshTitlebar()
    {
        var titleBar = ModMain.frmMain;
        if (!ThemeService.IsClassicThemeMode)
        {
            titleBar.RectTitleGradient.Fill = null;
            titleBar.ImgTitle.Source = string.Empty;
            titleBar.ImgTitle.Opacity = 1d;
            return;
        }

        var palette = PclThemeRuntime.RefreshPalette();

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0d, 0d),
            EndPoint = new Point(1d, 0d)
        };
        foreach (var stop in palette.TitlebarStops)
            brush.GradientStops.Add(new GradientStop(stop.Color.ToWpfColor(), stop.Offset));
        brush.Freeze();
        titleBar.RectTitleGradient.Fill = brush;

        titleBar.ImgTitle.Source = palette.HasBanner
            ? ModBase.pathImage + $"Themes/{palette.ThemeId}.png"
            : string.Empty;
        titleBar.ImgTitle.Opacity = palette.BannerOpacity;
    }
    
    // 主页面背景
    private static void RefreshBackground()
    {
        if (ThemeService.IsClassicThemeMode)
        {
            if (Config.Preference.Background.BackgroundColorful)
            {
                var palette = PclThemeRuntime.RefreshPalette();
                var brush = new LinearGradientBrush
                {
                    EndPoint = new Point(0.1, 1),
                    StartPoint = new Point(0.9, 0)
                };

                foreach (var stop in palette.BackgroundStops)
                    brush.GradientStops.Add(new GradientStop(stop.Color.ToWpfColor(), stop.Offset));

                ModMain.frmMain.PanForm.Background = brush;
            }
            else
            {
                // 原版经典主题的纯色背景
                ModMain.frmMain.PanForm.Background = new SolidColorBrush(Color.FromRgb(250, 250, 250));
            }
        }
        else if (Config.Preference.Background.BackgroundColorful)
        {
            var brush = new LinearGradientBrush
            {
                EndPoint = new Point(0.1, 1),
                StartPoint = new Point(0.9, 0)
            };

            var hue = ThemeService.GetCurrentThemeArgs().Hue;
            var hue1 = hue - 15;
            var hue2 = hue + 15;
            var tone = ThemeService.CurrentTone;
            var darkLight = IsDarkMode ? 0.2d : 1d;
            brush.GradientStops.Add(new GradientStop
                { Offset = -0.1d, Color = LabColor.FromLch(0.84d * darkLight, tone.C5, hue1) });
            brush.GradientStops.Add(new GradientStop
                { Offset = 0.4d, Color = LabColor.FromLch(0.96d * darkLight, tone.C7, hue) });
            brush.GradientStops.Add(new GradientStop
                { Offset = 1.1d, Color = LabColor.FromLch(0.84d * darkLight, tone.C5, hue2) });
            ModMain.frmMain.PanForm.Background = brush;
        }
        else
        {
            ModMain.frmMain.PanForm.Background = (Brush)System.Windows.Application.Current.Resources["ColorBrushBackground"];
        }

        ModMain.frmMain.PanForm.Background.Freeze();
    }

    // 通用ContextMenu主题刷新
    private static void RefreshAllContextMenuThemes()
    {
        try
        {
            if (!_contextMenuHandlerRegistered)
            {
                EventManager.RegisterClassHandler(typeof(ContextMenu), ContextMenu.OpenedEvent,
                    new RoutedEventHandler(OnContextMenuOpened));
                _contextMenuHandlerRegistered = true;
            }

            foreach (Window window in System.Windows.Application.Current.Windows)
                RefreshContextMenusInElement(window);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "刷新ContextMenu主题时出错");
        }
    }

    private static void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is ContextMenu contextMenu)
            {
                contextMenu.ClearValue(FrameworkElement.StyleProperty);
                contextMenu.UpdateDefaultStyle();
            }
        }
        catch
        {
            // 忽略个别错误
        }
    }

    private static void RefreshContextMenusInElement(DependencyObject element)
    {
        if (element is null)
            return;

        try
        {
            if (element is FrameworkElement { ContextMenu: not null } fe)
            {
                fe.ContextMenu.ClearValue(FrameworkElement.StyleProperty);
                fe.ContextMenu.UpdateDefaultStyle();
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
                RefreshContextMenusInElement(VisualTreeHelper.GetChild(element, i));
        }
        catch
        {
            // 忽略个别元素的错误，继续处理其他元素
        }
    }
}