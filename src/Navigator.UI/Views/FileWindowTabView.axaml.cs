using Avalonia.Controls;
using Avalonia.Input;
using Navigator.UI.Models;
using Navigator.UI.Models.Nodes;
using Navigator.UI.ViewModels;

namespace Navigator.UI.Views;

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

