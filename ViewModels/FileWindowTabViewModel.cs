using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<DirectoryNode> _treeViewItems = [];

    [ObservableProperty]
    private DirectoryNode? _selectedTreeNode;

    public FileWindowTabViewModel(FileWindowTab model)
    {
        TreeViewItems = model.RootFolders;
    }

    partial void OnSelectedTreeNodeChanged(DirectoryNode? value)
    {
        Logger.Debug($"FileWindowTabViewModel.SelectedTreeNode changed to: {value?.Name ?? "null"}");
    }
}

