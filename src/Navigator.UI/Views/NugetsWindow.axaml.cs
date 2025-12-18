using System.Text;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;

namespace Navigator.UI.Views;

public partial class NugetsWindow : Window {

     public record PackageInfo(string Name, string Version, string AssemblyName);

     public NugetsWindow() {
         InitializeComponent();

         RefreshButton.Click += (_, _) => RefreshList();
         CopyButton.Click += (_, _) => CopySelected();
         CloseButton.Click += (_, _) => Close();

         // Load initially
         Dispatcher.UIThread.Post(RefreshList);
     }

     private void InitializeComponent() {
         AvaloniaXamlLoader.Load(this);
     }

     private void RefreshList() {
         try {
             var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                 .Where(a => !a.IsDynamic)
                 .OrderBy(a => a.GetName().Name ?? string.Empty)
                 .ToList();

             var items = new List<PackageInfo>();
             foreach (var asm in assemblies) {
                 var name = asm.GetName();
                 var ver = name.Version?.ToString() ?? "n/a";
                 var asmName = !string.IsNullOrEmpty(asm.Location) ? asm.Location : (name.Name ?? string.Empty);
                 var displayName = name.Name ?? asmName;
                 items.Add(new PackageInfo(displayName, ver, asmName));
             }

             PackagesList.ItemsSource = items;
         } catch (Exception ex) {
             PackagesList.ItemsSource = new List<PackageInfo> { new PackageInfo("Error", ex.Message, string.Empty) };
         }
     }

     private void CopySelected() {
         string textToCopy = string.Empty;
         if (PackagesList.SelectedItem is PackageInfo pi) {
             var sb = new StringBuilder();
             sb.AppendLine($"{pi.Name} {pi.Version}");
             textToCopy = sb.ToString();
         } else {
             // Copy all
             if (PackagesList.ItemsSource is IEnumerable<PackageInfo> list) {
                 var sb = new StringBuilder();
                 foreach (var i in list) {
                     sb.AppendLine($"{i.Name} {i.Version}    {i.AssemblyName}");
                 }
                 textToCopy = sb.ToString();
             }
         }

         if (!string.IsNullOrEmpty(textToCopy)) {
             TryWriteToClipboard(textToCopy);
         }
     }

     private static void TryWriteToClipboard(string text) {
         try {
             if (OperatingSystem.IsMacOS()) {
                 WriteToProcess("pbcopy", text);
             } else if (OperatingSystem.IsWindows()) {
                 WriteToProcess("cmd.exe", "/c clip", text);
             } else {
                 // linux / unix - try wl-copy then xclip
                 if (!WriteToProcess("wl-copy", text)) {
                     if (!WriteToProcess("xclip", "-selection clipboard", text)) {
                         // as fallback, write to stdout of 'cat' to hopefully allow redirection
                         WriteToProcess("/bin/sh", "-c 'cat | xclip -selection clipboard'", text);
                     }
                 }
             }
         } catch {
             // ignore
         }
     }

     private static bool WriteToProcess(string fileName, string argsOrText, string? maybeText = null) {
         try {
             var psi = new ProcessStartInfo {
                 FileName = fileName,
                 UseShellExecute = false,
                 RedirectStandardInput = true,
             };

             if (maybeText is null) {
                 // argsOrText is actually text
                 psi.Arguments = string.Empty;
             } else {
                 psi.Arguments = argsOrText;
             }

             using var p = Process.Start(psi);
             if (p is null) return false;

             var toWrite = maybeText ?? argsOrText;
             if (!string.IsNullOrEmpty(toWrite)) {
                 p.StandardInput.Write(toWrite);
                 p.StandardInput.Close();
             }
             p.WaitForExit(2000);
             return true;
         } catch {
             return false;
         }
     }
}
