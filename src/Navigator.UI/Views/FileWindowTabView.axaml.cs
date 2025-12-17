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
        // Use the ViewModel instance already created by the model instead of
        // creating a second ViewModel. This ensures bindings that reference
        // "ViewModel" on the model (e.g. ViewModel.CurrentFolderName) observe
        // the same object that the view updates.
        DataContext = model.ViewModel;
    }

    private void BaseNode_DoubleTapped(object? sender, TappedEventArgs e) {
        if (DataContext is FileWindowTabViewModel vm && sender is Control control && control.DataContext is BaseNode node) {
            vm.DoubleClickItemCommand.Execute(node);
        }
    }
}
