This folder contains the source projects for platform-specific launchers and shared UI:

- Navigator.UI: Shared UI library (Views, ViewModels, Resources).
- Navigator.Mac: macOS-specific launcher (already present).
- Navigator.Windows: Windows-specific launcher (new).
- Navigator.Linux: Linux-specific launcher (new).

Guidance:
- Put shared XAML, controls, and view models into `Navigator.UI`.
- Platform projects reference `Navigator.UI` via a ProjectReference and add any platform-specific packages (e.g., `Avalonia.Desktop`).
- Adjust RuntimeIdentifier per target machine for self-contained publish.

