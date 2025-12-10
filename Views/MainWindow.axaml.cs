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
}
