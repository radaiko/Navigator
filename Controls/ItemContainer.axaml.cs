using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.Controls;

/// <summary>
/// Custom ItemContainer UserControl for displaying file items with hover effects and context menu
/// </summary>
public partial class ItemContainer : UserControl {
    private BaseNode? _item;
    private Grid? _contentGrid;
    private Border? _itemBorder;

    public event Action<BaseNode>? ItemSingleClicked;
    public event Action<BaseNode>? ItemDoubleClicked;
    public event Action<BaseNode>? ContextMenuRequested;

    public static readonly StyledProperty<BaseNode?> ItemProperty =
        AvaloniaProperty.Register<ItemContainer, BaseNode?>(
            nameof(Item),
            defaultValue: null);

    public BaseNode? Item {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ItemContainer() {
        InitializeComponent();
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("ItemContainer OnInitialized called");
        InitializeControls();
        SetupEventHandlers();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == ItemProperty) {
            _item = change.NewValue as FileNode;
            if (_item != null) {
                Logger.Debug($"ItemContainer: Binding FileItem - {_item.Name}");
                UpdateItemDisplay(_item);
            }
        }
    }

    private void InitializeControls() {
        _contentGrid = this.FindControl<Grid>("ContentGrid");
        _itemBorder = this.FindControl<Border>("ItemBorder");

        if (_contentGrid == null) {
            Logger.Warning("ContentGrid control not found");
        }
        if (_itemBorder == null) {
            Logger.Warning("ItemBorder control not found");
        }
    }

    private void SetupEventHandlers() {
        Rectangle? clickRect = this.FindControl<Rectangle>("ClickRect");
        if (clickRect != null) {
            clickRect.PointerPressed += OnPointerPressed;
        }

        // Context menu
        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem {
            Header = "Open",
            Command = new DelegateCommand(OnContextMenuOpen)
        });
        contextMenu.Items.Add(new MenuItem {
            Header = "Copy",
            Command = new DelegateCommand(OnContextMenuCopy)
        });
        contextMenu.Items.Add(new MenuItem {
            Header = "Delete",
            Command = new DelegateCommand(OnContextMenuDelete)
        });
        ContextMenu = contextMenu;

        Logger.Debug("ItemContainer event handlers setup complete");
    }

    private void UpdateItemDisplay(BaseNode item) {
        TextBlock? iconBlock = this.FindControl<TextBlock>("IconBlock");
        TextBlock? nameBlock = this.FindControl<TextBlock>("NameBlock");
        TextBlock? sizeBlock = this.FindControl<TextBlock>("SizeBlock");
        TextBlock? typeBlock = this.FindControl<TextBlock>("TypeBlock");
        TextBlock? modifiedBlock = this.FindControl<TextBlock>("ModifiedBlock");

        iconBlock?.Text = item.Icon;
        nameBlock?.Text = item.Name;
        if (item is FileNode fileItem) {
            sizeBlock?.Text = fileItem.FormattedSize;
            typeBlock?.Text = fileItem.Type;
            modifiedBlock?.Text = fileItem.LastModified;
        }


        Logger.Debug($"ItemContainer display updated for: {item.Name}");
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (_item == null) return;

        PointerPointProperties props = e.GetCurrentPoint(this).Properties;

        if (props.IsLeftButtonPressed) {
            e.Handled = true;
            if (e.ClickCount >= 2) {
                // Double-click
                ItemDoubleClicked?.Invoke(_item);
                Logger.Debug($"Double click on item: {_item.Name}");
            } else {
                // Single click
                ItemSingleClicked?.Invoke(_item);
                Logger.Debug($"Single click on item: {_item.Name}");
            }
        } else if (props.IsRightButtonPressed) {
            e.Handled = false;
            Logger.Debug($"Write click on item: {_item.Name}");
            ContextMenuRequested?.Invoke(_item);
        }
    }

    private void OnContextMenuOpen() {
        if (_item != null) {
            Logger.Info($"Context menu: Open - {_item.Name}");
            // TODO: Implement actual open logic
        }
    }

    private void OnContextMenuCopy() {
        if (_item != null) {
            Logger.Info($"Context menu: Copy - {_item.Name}");
            // TODO: Implement actual copy logic
        }
    }

    private void OnContextMenuDelete() {
        if (_item != null) {
            Logger.Info($"Context menu: Delete - {_item.Name}");
            // TODO: Implement actual delete logic
        }
    }
}

/// <summary>
/// Simple command implementation for context menu actions
/// </summary>
internal class DelegateCommand(Action execute) : ICommand {
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) {
        execute();
    }
}



