using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase {
    [ObservableProperty] private DirectoryNode? _selectedTreeNode;

    [ObservableProperty] private RootFolders _treeViewItems;

    [ObservableProperty] private ImmutableArray<BaseNode> _currentNodeChildren = [];

    public FileWindowTabViewModel(FileWindowTab model) {
        Logger.Info("Initializing FileWindowTabViewModel");

        TreeViewItems = new RootFolders();
        CurrentNodeChildren = TreeViewItems.ActualNode.Children;

        // Subscribe to ActualNode changes
        TreeViewItems.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(RootFolders.ActualNode)) {
                CurrentNodeChildren = TreeViewItems.ActualNode.Children;
                Logger.Debug($"Updated CurrentNodeChildren to: {CurrentNodeChildren.Length} items");
            }
        };

        Logger.Debug("FileWindowTabViewModel initialized successfully");
    }

    partial void OnSelectedTreeNodeChanged(DirectoryNode? value) {
        if (value != null) {
            TreeViewItems.GoToPath(value.Path);
            Logger.Debug($"FileWindowTabViewModel.SelectedTreeNode changed to: {value?.Path ?? "null"}");
        }
    }

    public void DoubleClickItem(DirectoryNode node) {
        TreeViewItems.GoToPath(node.Path);
        Logger.Debug($"Double-clicked on directory: {node.Path}");
    }
}
