using Avalonia.Controls;
using Navigator.UI.Utils;

namespace Navigator.UI.Views;

public partial class ExceptionWindow : Window {
    private Exception _exception;

    public string ExceptionContent { get; init; }

    public ExceptionWindow(Exception ex) {
        InitializeComponent();
        _exception = ex;
        ExceptionContent = ex.ToFormattedString();
    }
}
