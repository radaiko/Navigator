using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase {
    [ObservableProperty] private DirectoryNode? _selectedTreeNode;

    [ObservableProperty] private RootFolders _treeViewItems;

    public FileWindowTabViewModel(FileWindowTab model) {
        Logger.Info("Initializing FileWindowTabViewModel");

        TreeViewItems = new RootFolders();

        Logger.Debug("FileWindowTabViewModel initialized successfully");
    }

    partial void OnSelectedTreeNodeChanged(DirectoryNode? value) {
        Logger.Debug($"FileWindowTabViewModel.SelectedTreeNode changed to: {value?.Path ?? "null"}");
    }

    public void DoubleClickItem(DirectoryNode node) {
        TreeViewItems.GoToPath(node.Path);
        Logger.Debug($"Double-clicked on directory: {node.Path}");
    }
}
