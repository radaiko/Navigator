using Avalonia.Controls;
using Navigator.UI.Models;

namespace Navigator.UI.Views;

public partial class SettingsView : UserControl {
    public SettingsView() {
        InitializeComponent();
    }

    public SettingsView(SettingsTab model) : this() {
        DataContext = model.ViewModel;
    }
}
