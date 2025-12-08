using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.Controls;

public partial class TableView : UserControl {
    private readonly ObservableCollection<BaseNode> _items = [];

    // ReSharper disable once MemberCanBePrivate.Global
    // Need to be public for Avalonia property system
    public static readonly StyledProperty<DirectoryNode?> CurrentNodeProperty =
        AvaloniaProperty.Register<TableView, DirectoryNode?>(
            nameof(CurrentNode),
            defaultValue: null,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public DirectoryNode? CurrentNode {
        get => GetValue(CurrentNodeProperty);
        set => SetValue(CurrentNodeProperty, value);
    }

    public ObservableCollection<BaseNode> Items => _items;

    public TableView() {
        InitializeComponent();
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("TableView OnInitialized called");
        this.FindControl<ItemsControl>("FileItemsContainer");

        if (CurrentNode != null) {
            LoadDirectory(CurrentNode);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == CurrentNodeProperty) {
            if (change.NewValue is DirectoryNode newNode) {
                Logger.Info($"CurrentPath property changed to: {newNode.Path}");
                LoadDirectory(newNode);
            }
        }
    }

    private void LoadDirectory(DirectoryNode node) {
        node.UpdateChildren(true);
        foreach (BaseNode item in node.Children) {
            _items.Add(item);
        }
    }
}


