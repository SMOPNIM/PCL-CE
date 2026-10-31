using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PCL.Phase0.AvaloniaProbe;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RuntimeInfo.Text = $"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture}) / " +
                           $"Framework: {RuntimeInformation.FrameworkDescription}";
    }

    private async void OpenDialogProbe(object? sender, RoutedEventArgs e)
    {
        var input = new TextBox
        {
            PlaceholderText = "对话框输入框：打开后直接输入中文",
            Margin = new Avalonia.Thickness(0, 12, 0, 0)
        };

        var dialog = new Window
        {
            Title = "PCL Phase 0 / Dialog IME Probe",
            Width = 520,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = "此对话框打开后会自动聚焦输入框。请直接切换输入法并输入中文。",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    input
                }
            }
        };

        dialog.Opened += (_, _) => input.Focus();
        await dialog.ShowDialog(this);
    }
}
