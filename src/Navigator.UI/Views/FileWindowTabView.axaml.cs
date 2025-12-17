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

    private void BaseNode_ContextRequested(object? sender, ContextRequestedEventArgs e) {
        if (DataContext is FileWindowTabViewModel vm && sender is Control control && control.DataContext is BaseNode node) {
            vm.SelectedNode = node;
        }
    }

    private void BaseNode_PointerPressed(object? sender, PointerPressedEventArgs e) {
        // Only set selection on right-click (secondary button)
        if (e.GetCurrentPoint(null).Properties.IsRightButtonPressed) {
            if (DataContext is FileWindowTabViewModel vm && sender is Control control && control.DataContext is BaseNode node) {
                vm.SelectedNode = node;
            }
        }
    }

    // Helper to resolve the BaseNode associated with a ContextMenu/MenuItem click
    private static BaseNode? ResolveNodeFromMenuItem(object? sender) {
        if (sender is MenuItem menuItem) {
            // Parent should be ContextMenu
            if (menuItem.Parent is ContextMenu ctx) {
                // PlacementTarget is the control the context menu is attached to
                if (ctx.PlacementTarget is Control target && target.DataContext is BaseNode node) {
                    return node;
                }
            }

            // Fallback: MenuItem.DataContext may be set to the node in some cases
            if (menuItem.DataContext is BaseNode md) return md;
        }
        return null;
    }

    private void ContextMenu_Open_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.OpenCommand.Execute(node);
    }

    private void ContextMenu_OpenInNewTab_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        // For now, call Open as there is no separate OpenInNewTab implementation
        if (DataContext is FileWindowTabViewModel vm) vm.OpenCommand.Execute(node);
    }

    private void ContextMenu_NewFolder_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.NewFolderCommand.Execute(node);
    }

    private void ContextMenu_NewFolder_Empty_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (DataContext is FileWindowTabViewModel vm) {
            var current = vm.Root.ActualNode;
            if (current != null) vm.NewFolderCommand.Execute(current);
        }
    }

    private void ContextMenu_Rename_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.RenameCommand.Execute(node);
    }

    private void ContextMenu_Delete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.DeleteCommand.Execute(node);
    }

    private void ContextMenu_CopyPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.CopyPathCommand.Execute(node);
    }

    private void ContextMenu_Properties_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var node = ResolveNodeFromMenuItem(sender);
        if (node == null) return;
        if (DataContext is FileWindowTabViewModel vm) vm.PropertiesCommand.Execute(node);
    }
}
