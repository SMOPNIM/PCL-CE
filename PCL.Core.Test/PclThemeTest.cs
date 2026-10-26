using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCL.Core.UI.Theme;

namespace PCL.Core.Test;

[TestClass]
public class PclThemeTest
{
    [TestMethod]
    public void TestTheme0ReferenceColors()
    {
        var definition = PclThemeCatalog.GetById(0);
        Assert.IsNotNull(definition);

        var palette = PclThemePalette.Create(definition, definition.Hue);

        AssertColor(51, 62, 72, palette.Color1);
        AssertColor(15, 100, 184, palette.Color2);
        AssertColor(18, 122, 225, palette.Color3);
        AssertColor(60, 150, 239, palette.Color4);
        AssertColor(144, 195, 246, palette.Color5);
        AssertColor(205, 228, 251, palette.Color6);
        AssertColor(222, 237, 252, palette.Color7);
        AssertColor(238, 246, 254, palette.Color8);
        AssertColor(115, 171, 227, palette.Background0);
    }

    [TestMethod]
    public void TestTheme0DerivedResources()
    {
        var definition = PclThemeCatalog.GetById(0)!;
        var palette = PclThemePalette.Create(definition, definition.Hue);

        Assert.AreEqual(190, palette.Background1.AlphaByte);
        Assert.AreEqual(1, palette.SemiTransparent.AlphaByte);
        AssertColor(222, 237, 252, palette.Background1);
        AssertColor(238, 246, 254, palette.SemiTransparent);
    }

    [TestMethod]
    public void TestTheme0TitlebarAndBackgroundStops()
    {
        var definition = PclThemeCatalog.GetById(0)!;
        var palette = PclThemePalette.Create(definition, definition.Hue);

        Assert.AreEqual(3, palette.TitlebarStops.Count);
        AssertColor(16, 106, 196, palette.TitlebarStops[0].Color);
        AssertColor(18, 119, 221, palette.TitlebarStops[1].Color);
        AssertColor(16, 106, 196, palette.TitlebarStops[2].Color);

        Assert.AreEqual(3, palette.BackgroundStops.Count);
        Assert.AreEqual(-0.1d, palette.BackgroundStops[0].Offset);
        Assert.AreEqual(0.4d, palette.BackgroundStops[1].Offset);
        Assert.AreEqual(1.1d, palette.BackgroundStops[2].Offset);
        AssertColor(177, 207, 213, palette.BackgroundStops[0].Color);
        AssertColor(203, 225, 248, palette.BackgroundStops[1].Color);
        AssertColor(192, 197, 221, palette.BackgroundStops[2].Color);
    }

    [TestMethod]
    public void TestTitlebarModes()
    {
        var theme5 = PclThemePalette.Create(PclThemeCatalog.GetById(5)!, 225);
        Assert.AreEqual(PclTitlebarMode.FiveStop, PclThemeCatalog.GetById(5)!.TitlebarMode);
        Assert.AreEqual(3, theme5.TitlebarStops.Count);
        Assert.AreEqual(0d, theme5.TitlebarStops[0].Offset);
        Assert.AreEqual(0.5d, theme5.TitlebarStops[1].Offset);
        Assert.AreEqual(1d, theme5.TitlebarStops[2].Offset);

        var theme12 = PclThemePalette.Create(PclThemeCatalog.GetById(12)!, 90);
        Assert.AreEqual(PclTitlebarMode.FourStop, PclThemeCatalog.GetById(12)!.TitlebarMode);
        Assert.AreEqual(4, theme12.TitlebarStops.Count);
        Assert.AreEqual(0.33d, theme12.TitlebarStops[1].Offset);
        Assert.AreEqual(0.67d, theme12.TitlebarStops[2].Offset);
    }

    [TestMethod]
    public void TestTheme14CustomSettings()
    {
        var definition = PclThemeCatalog.GetById(14)!;
        var palette = PclThemePalette.Create(definition, 180,
            new PclThemeCustomSettings(Hue: 180, Saturation: 80, LightAdjust: 0, TitlebarDelta: 0));

        AssertColor(PclColor.FromHsl2(180, 80, 65), palette.Color4);
        Assert.AreEqual(3, palette.TitlebarStops.Count);
        AssertColor(PclColor.FromHsl2(180, 80, 48), palette.TitlebarStops[0].Color);
        AssertColor(PclColor.FromHsl2(180, 80, 54), palette.TitlebarStops[1].Color);
        AssertColor(PclColor.FromHsl2(180, 80, 48), palette.TitlebarStops[2].Color);

        var gradient = PclThemePalette.Create(definition, 180,
            new PclThemeCustomSettings(Hue: 180, Saturation: 80, LightAdjust: 0, TitlebarDelta: 90));
        AssertColor(PclColor.FromHsl2(90, 80, 48), gradient.TitlebarStops[0].Color);
        AssertColor(PclColor.FromHsl2(180, 80, 54), gradient.TitlebarStops[1].Color);
        AssertColor(PclColor.FromHsl2(270, 80, 48), gradient.TitlebarStops[2].Color);
    }

    [TestMethod]
    public void TestCatalogLayout()
    {
        Assert.AreEqual(16, PclThemeCatalog.All.Count);
        Assert.AreEqual(15, PclThemeCatalog.Selectable.Count);

        Assert.IsNull(PclThemeCatalog.GetById(42)!.SelectorRow);
        Assert.IsNull(PclThemeCatalog.GetById(42)!.SelectorColumn);
        Assert.IsTrue(PclThemeCatalog.GetById(42)!.HasBanner);

        for (var id = 0; id <= 14; id++)
        {
            var definition = PclThemeCatalog.GetById(id);
            Assert.IsNotNull(definition);
            Assert.IsNotNull(definition.SelectorRow);
            Assert.IsNotNull(definition.SelectorColumn);
        }

        Assert.AreEqual(1, PclThemeCatalog.GetById(0)!.SelectorColumn);
        Assert.AreEqual(0, PclThemeCatalog.GetById(0)!.SelectorRow);
        Assert.AreEqual(2, PclThemeCatalog.GetById(12)!.SelectorColumn);
        Assert.AreEqual(1, PclThemeCatalog.GetById(12)!.SelectorRow);
        Assert.AreEqual(5, PclThemeCatalog.GetById(14)!.SelectorColumn);
        Assert.AreEqual(2, PclThemeCatalog.GetById(14)!.SelectorRow);

        int[] bannerIds = [5, 6, 7, 8, 9, 10, 11, 12, 13, 42];
        foreach (var definition in PclThemeCatalog.All)
            Assert.AreEqual(bannerIds.Contains(definition.Id), definition.HasBanner, $"banner {definition.Id}");

        Assert.AreEqual(0.25d, PclThemeCatalog.GetById(13)!.BannerOpacity);
        Assert.AreEqual(0.5d, PclThemeCatalog.GetById(12)!.BannerOpacity);
    }

    private static void AssertColor(byte red, byte green, byte blue, PclColor color) =>
        AssertColor(new PclColor(red, green, blue), color);

    private static void AssertColor(PclColor expected, PclColor actual) =>
        Assert.AreEqual(
            $"{expected.RedByte},{expected.GreenByte},{expected.BlueByte}",
            $"{actual.RedByte},{actual.GreenByte},{actual.BlueByte}");
}
