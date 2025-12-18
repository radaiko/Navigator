using System.Text;

namespace Navigator.UI.Utils;

public static class ExceptionExtensions {
    public static string ToFormattedString(this Exception ex) {
        var sb = new StringBuilder();
        var current = ex;
        int level = 0;

        while (current != null) {
            if (level > 0) {
                sb.AppendLine();
                sb.AppendLine($"--- Inner Exception Level {level} ---");
            } else {
                sb.AppendLine("--- Exception ---");
            }

            sb.AppendLine($"Type: {current.GetType().FullName}");
            sb.AppendLine($"Message: {current.Message}");

            if (!string.IsNullOrWhiteSpace(current.Source))
                sb.AppendLine($"Source: {current.Source}");

            if (current.TargetSite != null)
                sb.AppendLine($"TargetSite: {current.TargetSite}");

            if (!string.IsNullOrWhiteSpace(current.StackTrace)) {
                sb.AppendLine("StackTrace:");
                sb.AppendLine(current.StackTrace);
            }

            current = current.InnerException;
            level++;
        }

        return sb.ToString();
    }
}
