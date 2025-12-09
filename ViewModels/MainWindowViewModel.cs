using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.Models;

namespace Navigator.ViewModels;

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
}
