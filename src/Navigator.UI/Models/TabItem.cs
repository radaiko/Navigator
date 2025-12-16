using Avalonia.Controls;

namespace Navigator.UI.Models;

public abstract class TabItem {
    public string Title { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;

    public Control? ContentControl { get; set; }

    public virtual void OnActivated() { }
    public virtual void OnDeactivated() { }
    public virtual void OnClosed() { }
}

