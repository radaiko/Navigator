using Avalonia.Controls;
using Avalonia.Interactivity;
using Navigator.UI.Utils;

namespace Navigator.UI.Views;

public partial class ExceptionWindow : Window {
    public ExceptionWindow(Exception ex) {
        InitializeComponent();
        ExceptionContent.Text = ex.ToFormattedString();
    }

    private void CopyButtonClicked(object? sender, RoutedEventArgs e) {
        try {
            var text = ExceptionContent?.Text ?? string.Empty;

            // Prefer a cross-platform approach similar to NugetsWindow to avoid relying on Avalonia's Clipboard symbol
            if (!string.IsNullOrEmpty(text)) {
                ClipboardHelper.SetTextAsync(text);
            }
        } catch {
            // Swallow exceptions to avoid throwing from a UI event handler
        }
    }
}
