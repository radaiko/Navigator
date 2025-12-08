using Avalonia.Controls;

namespace Navigator.Models;

public abstract class TabItem
{
    public string Title { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    
    /// <summary>
    /// The custom content control for this tab. Should be set by derived classes.
    /// </summary>
    public Control? ContentControl { get; set; }
    
    public virtual void OnActivated() { }
    public virtual void OnDeactivated() { }
    public virtual void OnClosed() { }
}

