using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase {

    [ObservableProperty] private RootFolders _root;

    [ObservableProperty] private ImmutableArray<BaseNode> _currentNodeChildren = [];

    public FileWindowTabViewModel(FileWindowTab model) {
        Logger.Info("Initializing FileWindowTabViewModel");

        Root = new RootFolders();
        CurrentNodeChildren = Root.ActualNode.Children;

        // Subscribe to ActualNode changes
        Root.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(RootFolders.ActualNode)) {
                CurrentNodeChildren = Root.ActualNode.Children;
                Logger.Debug($"Updated CurrentNodeChildren to: {CurrentNodeChildren.Length} items");
            }
        };

        Logger.Debug("FileWindowTabViewModel initialized successfully");
    }

    [RelayCommand]
    public void DoubleClickItem(BaseNode node) {
        Logger.Debug($"Double-clicked on directory: {node.Path}");
        if (node is not DirectoryNode directory) {
            Logger.Debug($"Item is not a directory. No action taken.");
            return;
        }

        Root.GoToPath(directory.Path);
        Logger.Debug($"Navigated to directory: {directory.Path}");
    }

    [RelayCommand]
    public void BackClicked() {
        Root.GoBack();
        Logger.Debug($"Back clicked");
    }

    [RelayCommand]
    public void UpClicked() {
        Root.GoUp();
        Logger.Debug($"Up clicked");
    }

    [RelayCommand]
    public void ForwardClicked() {
        Root.GoForward();
        Logger.Debug($"Forward clicked");
    }

    [RelayCommand]
    public void RefreshClicked() {
        Root.Refresh();
        Logger.Debug($"Refresh clicked");
    }
}
