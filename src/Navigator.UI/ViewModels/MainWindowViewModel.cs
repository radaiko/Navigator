using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.UI.Models;

namespace Navigator.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
    [ObservableProperty] private TabItem? _selectedTab;

    [ObservableProperty] private ObservableCollection<TabItem> _tabs = [];

    public MainWindowViewModel() {
        Tabs = [];
    }

    public void AddTab(TabItem tab) {
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(TabItem tab) {
        if (Tabs.Contains(tab)) {
            tab.OnClosed();
            Tabs.Remove(tab);
        }
    }

    // Command to add a new FileWindowTab (used by menus / shortcuts)
    [RelayCommand]
    private void AddNewTab() {
        var fileTab = new FileWindowTab();
        AddTab(fileTab);
    }

    // Command to close the currently selected tab
    [RelayCommand]
    private void CloseActiveTab() {
        if (SelectedTab is { } tab) {
            CloseTab(tab);
        }
    }

    [RelayCommand]
    private void SwitchToNextTab() {
        if (Tabs.Count == 0 || SelectedTab is null) return;

        var currentIndex = Tabs.IndexOf(SelectedTab);
        var nextIndex = (currentIndex + 1) % Tabs.Count;
        SelectedTab = Tabs[nextIndex];
    }

    [RelayCommand]
    private void SwitchToPreviousTab()
    {
        if (Tabs.Count == 0 || SelectedTab is null) return;
        var currentIndex = Tabs.IndexOf(SelectedTab);
        var previousIndex = (currentIndex - 1 + Tabs.Count) % Tabs.Count;
        SelectedTab = Tabs[previousIndex];
    }
}
