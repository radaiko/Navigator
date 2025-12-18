using Avalonia.Controls;
using Avalonia.Input;
using Navigator.UI.Converters;
using Navigator.UI.Models;
using Navigator.UI.ViewModels;
using System.Runtime.InteropServices;
using TabItem = Navigator.UI.Models.TabItem;

namespace Navigator.UI.Views;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        AddHandler(PointerPressedEvent, TitleBar_PointerPressed, handledEventsToo: true);
    }

    private void OnCloseTabClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (sender is Button button && button.Tag is TabItem tabItem) {
            if (DataContext is MainWindowViewModel viewModel) {
                viewModel.CloseTab(tabItem);
            }
        }
    }

    private void OnMinimizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (WindowState == WindowState.Maximized) {
            WindowState = WindowState.Normal;
        } else {
            WindowState = WindowState.Maximized;
        }
    }

    private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Close();
    }

    private void TitleBar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            var point = e.GetCurrentPoint(this);
            if (point.Position.Y < 24) {
                string platform = OSPlatformConverter.GetCurrentPlatform();
                bool shouldDrag = false;

                if (platform == "macOS") {
                    double leftButtonsWidth = 12 + 8 + 12 + 8 + 12 + 16;
                    shouldDrag = point.Position.X > leftButtonsWidth;
                } else {
                    double rightButtonsWidth = 3 * 36;
                    shouldDrag = point.Position.X < Bounds.Width - rightButtonsWidth;
                }

                if (shouldDrag) {
                    BeginMoveDrag(e);
                }
            }
        }
    }
}

