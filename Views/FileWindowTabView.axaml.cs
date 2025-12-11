using Avalonia.Controls;
using Avalonia.Input;
using Navigator.Models;
using Navigator.Models.Nodes;
using Navigator.ViewModels;

namespace Navigator.Views;

public partial class FileWindowTabView : UserControl {
    private readonly FileWindowTabViewModel? _viewModel;

    public FileWindowTabView() {
        InitializeComponent();
    }

    public FileWindowTabView(FileWindowTab model) : this() {
        _viewModel = new FileWindowTabViewModel(model);
        DataContext = _viewModel;
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e) {
        if (_viewModel?.SelectedTreeNode?.Children is null) {
            return;
        }

        // Get the data context of the double-clicked item
        if (sender is Border border && border.DataContext is DirectoryNode directoryNode) {
            // Set the selected tree node to the double-clicked directory
            _viewModel.DoubleClickItem(directoryNode);
            e.Handled = true;
        }
    }
}
