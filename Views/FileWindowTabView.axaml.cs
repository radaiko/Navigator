using Avalonia.Controls;
using Navigator.Models;
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
}
