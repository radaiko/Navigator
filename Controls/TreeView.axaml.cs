using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Navigator.Models.Nodes;

namespace Navigator.Controls;

public partial class TreeView : UserControl {
    private StackPanel? _rootContainer;
    private readonly Dictionary<DirectoryNode, TreeItemControl> _itemMap = new();

    public static readonly StyledProperty<ObservableCollection<DirectoryNode>> ItemsSourceProperty =
        AvaloniaProperty.Register<TreeView, ObservableCollection<DirectoryNode>>(
            nameof(ItemsSource));

    public static readonly StyledProperty<DirectoryNode?> SelectedNodeProperty =
        AvaloniaProperty.Register<TreeView, DirectoryNode?>(
            nameof(SelectedNode),
            defaultValue: null,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public ObservableCollection<DirectoryNode> ItemsSource {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DirectoryNode? SelectedNode {
        get => GetValue(SelectedNodeProperty);
        set {
            Logger.Debug($"TreeView.SelectedNode setter called with value: {value?.Name ?? "null"}");
            SetValue(SelectedNodeProperty, value);
        }
    }

    public TreeView() {
        InitializeComponent();
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("TreeView OnInitialized called");
        _rootContainer = this.FindControl<StackPanel>("RootContainer");
        if (_rootContainer != null) {
            Logger.Debug("RootContainer found successfully");
            // Rebuild tree now that the container is available
            if (ItemsSource.Count > 0) {
                Logger.Debug("Rebuilding tree now that RootContainer is initialized");
                RebuildTree();
            }
        } else {
            Logger.Warning("RootContainer not found in TreeView");
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty) {
            Logger.Info($"ItemsSource property changed");
            var newValue = change.NewValue as ObservableCollection<DirectoryNode>;
            Logger.Info($"ItemsSource set to collection with {newValue?.Count ?? 0} items");
            RebuildTree();
        }
    }

    private void RebuildTree() {
        Logger.Debug("RebuildTree called");
        if (_rootContainer == null) {
            Logger.Warning("RootContainer is null, cannot rebuild tree");
            return;
        }

        Logger.Debug($"Clearing {_rootContainer.Children.Count} existing tree items");
        _rootContainer.Children.Clear();
        _itemMap.Clear();

        Logger.Info($"Building tree with {ItemsSource.Count} root items");
        foreach (DirectoryNode item in ItemsSource) {
            Logger.Debug($"Adding root item: {item.Name}");
            AddTreeItem(item, _rootContainer, indentLevel: 0);
        }

        // Subscribe to collection changes
        if (ItemsSource is INotifyCollectionChanged notifyCollectionChanged) {
            Logger.Debug("Subscribing to ItemsSource collection changes");
            notifyCollectionChanged.CollectionChanged += ItemsSource_CollectionChanged;
        }

        Logger.Info($"Tree rebuild complete with {_itemMap.Count} total items");
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        Logger.Debug($"ItemsSource collection changed: Action={e.Action}");
        RebuildTree();
    }

    private void AddTreeItem(DirectoryNode node, Panel parent, int indentLevel) {
        Logger.Debug($"AddTreeItem: {node.Name} (indent level: {indentLevel})");
        var treeItemControl = new TreeItemControl(node, indentLevel);
        treeItemControl.NodeSelected += (selectedNode) => {
            Logger.Debug($"Node selected: {selectedNode.Name}");
            SelectedNode = selectedNode;
        };
        _itemMap[node] = treeItemControl;
        parent.Children.Add(treeItemControl);
        Logger.Debug($"Tree item added to parent. Total items in map: {_itemMap.Count}");
    }

}

/// <summary>
/// Represents a single item in the custom TreeView
/// </summary>
public class TreeItemControl : StackPanel {
    private readonly DirectoryNode _node;
    private readonly int _indentLevel;
    private Border? _expanderBorder;
    private TextBlock? _expanderText;
    private TextBlock? _nameText;
    private StackPanel? _childrenContainer;
    private bool _isExpanded;

    // Event to notify parent TreeView of selection
    public event Action<DirectoryNode>? NodeSelected;

    public TreeItemControl(DirectoryNode node, int indentLevel) {
        _node = node;
        _indentLevel = indentLevel;

        Logger.Debug($"Creating TreeItemControl for '{node.Name}' at indent level {indentLevel}");

        Spacing = 0;
        Orientation = Avalonia.Layout.Orientation.Vertical;

        BuildItemUi();
        SubscribeToNodeChanges();

        Logger.Debug($"TreeItemControl initialized for '{node.Name}'");
    }

    private void SubscribeToNodeChanges() {
        Logger.Debug($"Subscribing to node property changes for: {_node.Name}");
        _node.PropertyChanged += (sender, e) => {
            Logger.Debug($"PropertyChanged fired for '{_node.Name}', Property: '{e.PropertyName}'");
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(DirectoryNode.Children)) {
                Logger.Debug($"Children property changed for '{_node.Name}', Children count: {_node.Children.Length}, updating expander visibility");
                UpdateExpanderVisibility();
            }
        };
    }

    private void BuildItemUi() {
        Logger.Debug($"Building UI for TreeItemControl: {_node.Name}");

        // Item row (expander + name)
        var itemRow = new StackPanel {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            Height = 32,
            Margin = new Thickness(_indentLevel * 16, 0, 0, 0)
        };

        // Expander button
        _expanderBorder = new Border {
            Width = 32,
            Height = 32,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            BorderThickness = new Thickness(0),
            IsHitTestVisible = true,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        _expanderText = new TextBlock {
            Text = ">",
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            IsHitTestVisible = false
        };

        _expanderBorder.Child = _expanderText;

        // Name text
        _nameText = new TextBlock {
            Text = _node.Name,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        itemRow.Children.Add(_expanderBorder);
        itemRow.Children.Add(_nameText);

        // Set up expander click handler
        _expanderBorder.PointerPressed += Expander_PointerPressed;
        Logger.Debug($"Expander click handler attached to '{_node.Name}'");

        // Set up name text click handler for selection
        _nameText.PointerPressed += NameText_PointerPressed;
        Logger.Debug($"Name text click handler attached to '{_node.Name}'");

        // Children container (hidden by default)
        _childrenContainer = new StackPanel {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Spacing = 0
        };

        Children.Add(itemRow);
        Children.Add(_childrenContainer);

        UpdateExpanderVisibility();
        Logger.Debug($"UI built for '{_node.Name}'");
    }

    private void Expander_PointerPressed(object? sender, PointerPressedEventArgs e) {
        Logger.Debug($"Expander clicked for '{_node.Name}'");

        if (_expanderBorder == null) {
            Logger.Warning($"ExpanderBorder is null for '{_node.Name}'");
            return;
        }

        // Toggle expanded state
        _isExpanded = !_isExpanded;
        Logger.Info($"Toggled expanded state for '{_node.Name}' to {_isExpanded}");

        if (_isExpanded) {
            // If this is a Directory node, update children
            Logger.Debug($"Expanding directory '{_node.Name}', calling UpdateChildren()");
            _node.UpdateChildren(true);
            PopulateChildren(_node);
        }

        UpdateChildrenVisibility();
        e.Handled = true;
    }

    private void NameText_PointerPressed(object? sender, PointerPressedEventArgs e) {
        Logger.Debug($"Name text clicked for '{_node.Name}'");
        NodeSelected?.Invoke(_node);
        e.Handled = true;
    }

    private void PopulateChildren(DirectoryNode directoryNode) {
        if (_childrenContainer == null) {
            Logger.Warning($"ChildrenContainer is null for '{_node.Name}'");
            return;
        }

        Logger.Debug($"Populating children for '{_node.Name}' with {directoryNode.Children.Length} items");
        _childrenContainer.Children.Clear();

        foreach (BaseNode node in directoryNode.Children) {
            if (node is DirectoryNode child) {
                Logger.Debug($"Adding child: {child.Name}");
                var childControl = new TreeItemControl(child, _indentLevel + 1);
                _childrenContainer.Children.Add(childControl);
            }
        }
    }

    private void UpdateExpanderVisibility() {
        Logger.Debug($"UpdateExpanderVisibility called for '{_node.Name}'");

        if (_expanderBorder == null || _expanderText == null) {
            Logger.Warning($"Expander border or text is null for '{_node.Name}'");
            return;
        }

        Logger.Debug($"Checking children count for '{_node.Name}': {_node.Children.Length}");

        // Hide expander if directory has no children, but keep space reserved
        if (_node.Children.Length == 0) {
            _expanderBorder.Opacity = 0;
            _expanderBorder.IsHitTestVisible = false;
            _expanderText.Text = "-";
            Logger.Debug($"Expander hidden for '{_node.Name}' (no children)");
        } else {
            _expanderBorder.Opacity = 1;
            _expanderBorder.IsHitTestVisible = true;
            _expanderText.Text = _isExpanded ? "v" : ">";
            Logger.Debug($"Expander for '{_node.Name}' set to '{_expanderText.Text}' (IsExpanded: {_isExpanded})");

            // If expanded and children just became available, populate them
            if (_isExpanded && _childrenContainer?.Children.Count == 0) {
                Logger.Debug($"Children became available for '{_node.Name}', populating...");
                PopulateChildren(_node);
                if (_childrenContainer != null) {
                    _childrenContainer.IsVisible = true;
                    Logger.Debug($"Children container shown for '{_node.Name}'");
                }
            }
        }
    }

    private void UpdateChildrenVisibility() {
        Logger.Debug($"UpdateChildrenVisibility called for '{_node.Name}'");

        if (_childrenContainer == null) {
            Logger.Warning($"ChildrenContainer is null for '{_node.Name}'");
            return;
        }

        _childrenContainer.IsVisible = _isExpanded;
        Logger.Debug(_isExpanded
            ? $"Children container shown for '{_node.Name}'"
            : $"Children container hidden for '{_node.Name}'");

        UpdateExpanderVisibility();
    }
}

