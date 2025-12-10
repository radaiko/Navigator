using Avalonia.Controls;
using Avalonia.Input;
using Navigator.Models;
using Navigator.ViewModels;
using TabItem = Navigator.Models.TabItem;

namespace Navigator.Views;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        KeyDown += MainWindow_KeyDown;
        AddHandler(PointerPressedEvent, TitleBar_PointerPressed, handledEventsToo: true);
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e) {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.T) {
            e.Handled = true;
            AddNewTab();
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.W) {
            e.Handled = true;
            CloseActiveTab();
            return;
        }
    }

    private void AddNewTab() {
        if (DataContext is MainWindowViewModel viewModel) {
            var newTab = new FileWindowTab();
            viewModel.AddTab(newTab);
        }
    }

    private void OnCloseTabClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (sender is Button button && button.Tag is TabItem tabItem) {
            if (DataContext is MainWindowViewModel viewModel) {
                viewModel.CloseTab(tabItem);
            }
        }
    }

    private void CloseActiveTab() {
        if (DataContext is MainWindowViewModel viewModel) {
            if (viewModel.SelectedTab is TabItem selectedTab) {
                viewModel.CloseTab(selectedTab);
            }
        }
        // TODO: do not switch to the first one but the next one left if available
    }

    /// <summary>
    /// Minimizes the window
    /// </summary>
    private void OnMinimizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Toggles between maximized and normal window state
    /// </summary>
    private void OnMaximizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (WindowState == WindowState.Maximized) {
            WindowState = WindowState.Normal;
        } else {
            WindowState = WindowState.Maximized;
        }
    }

    /// <summary>
    /// Closes the window
    /// </summary>
    private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Close();
    }

    /// <summary>
    /// Handles pointer pressed event on the title bar to enable window dragging
    /// </summary>
    private void TitleBar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e) {
        // Only handle left mouse button
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            // Check if the click is on the title bar area (not on buttons)
            var point = e.GetCurrentPoint(this);
            if (point.Position.Y < 36) {
                // Exclude clicks on window control buttons (right side)
                // 3 buttons × 36px = 108px
                if (point.Position.X < Bounds.Width - 108) {
                    BeginMoveDrag(e);
                }
            }
        }
    }
}
