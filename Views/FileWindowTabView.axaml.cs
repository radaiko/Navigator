using Avalonia.Controls;
using Navigator.Controls;
using Navigator.Models;
using Navigator.ViewModels;

namespace Navigator.Views;

public partial class FileWindowTabView : UserControl
{
    private TableView? _fileTableView;
    private FileWindowTabViewModel? _viewModel;

    public FileWindowTabView()
    {
        InitializeComponent();
    }

    public FileWindowTabView(FileWindowTab model) : this()
    {
        _viewModel = new FileWindowTabViewModel(model);
        DataContext = _viewModel;
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("FileWindowTabView OnInitialized called");

        _fileTableView = this.FindControl<TableView>("FileTableView");

        if (_fileTableView != null && _viewModel != null) {
            // Subscribe to TreeView node selection changes
            var treeViewItems = _viewModel.TreeViewItems;
            if (treeViewItems.Count > 0) {
                // Load the home folder by default
                _fileTableView.CurrentNode = treeViewItems[0];
                Logger.Info($"Initialized TableView with default path: {treeViewItems[0].Path}");
            }
        } else {
            Logger.Warning("FileTableView or ViewModel not found during initialization");
        }
    }
}

