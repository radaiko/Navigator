using Avalonia.Controls;
using Avalonia.Input;
using Navigator.Models;
using Navigator.Models.Nodes;
using Navigator.ViewModels;

namespace Navigator.Views;

public partial class FileWindowTabView : UserControl {
    public FileWindowTabView() {
        InitializeComponent();
    }

    public FileWindowTabView(FileWindowTab model) : this() {
        DataContext = new FileWindowTabViewModel(model);
    }

    private void BaseNode_DoubleTapped(object? sender, TappedEventArgs e) {
        if (DataContext is FileWindowTabViewModel vm && sender is Control control && control.DataContext is BaseNode node) {
            vm.DoubleClickItemCommand.Execute(node);
        }
    }
}
