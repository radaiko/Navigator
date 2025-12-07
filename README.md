# Navigtor
A better file explorer for MacOS, Windows and Linux

## Project Structure

This is a cross-platform desktop application built with:
- **Avalonia UI 11.3.9** - Modern cross-platform UI framework
- **CommunityToolkit.MVVM 8.2.1** - MVVM helpers and source generators
- **.NET 9.0** - Target framework

### Directory Structure

```
Navigtor/
├── Assets/              # Application icons for all platforms
│   ├── navigtor.ico     # Windows icon
│   ├── navigtor.icns    # macOS icon
│   └── navigtor-*.png   # Linux icons (multiple sizes)
├── Models/              # Data models
├── ViewModels/          # MVVM ViewModels
│   ├── ViewModelBase.cs # Base class using ObservableObject
│   └── MainWindowViewModel.cs
├── Views/               # UI Views
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
├── App.axaml           # Application definition
├── Program.cs          # Entry point
└── ViewLocator.cs      # View resolution logic

```

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later

### Building

```bash
dotnet restore
dotnet build
```

### Running

```bash
dotnet run
```

## Platform-Specific Icons

The application includes placeholder icons for all supported platforms:
- **Windows**: `Assets/navigtor.ico` (multi-resolution .ico file)
- **macOS**: `Assets/navigtor.icns` (Apple icon format)
- **Linux**: `Assets/navigtor-256.png` (PNG format, multiple sizes available)

## License

See [LICENSE](LICENSE) file for details.
