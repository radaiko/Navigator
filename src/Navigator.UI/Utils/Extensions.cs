using System.Text;
using System.Text.RegularExpressions;

namespace Navigator.UI.Utils;

public static partial class ExceptionExtensions {
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
                // Replace full file paths like "in /path/to/File.cs:line 58" with "in File.cs:line 58"
                var sanitized = PathSanitizerRegex().Replace(current.StackTrace, "in ${file}:line ${line}");

                // Also strip fully-qualified type/namespace prefixes from the method, leaving only the method name and its arguments
                // Examples:
                //  - "at My.Namespace.MyClass.UpdateChildren(Boolean isRecursive) in File.cs:line 58"
                //    -> "at UpdateChildren(Boolean isRecursive) in File.cs:line 58"
                //  - "at My.Namespace.MyClass..ctor(String path, Boolean isRecursive) in File.cs:line 20"
                //    -> "at ctor(String path, Boolean isRecursive) in File.cs:line 20"
                sanitized = MethodSanitizerRegex().Replace(sanitized, "${prefix}${method}${args}");

                sb.AppendLine(sanitized);
            }

            current = current.InnerException;
            level++;
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"\bin\s+.*[\\/](?<file>[^\\/]+):line\s+(?<line>\d+)")]
    private static partial Regex PathSanitizerRegex();

    // Matches the beginning "at " prefix followed by any fully-qualified type/namespace segments and keeps only the final method name and its argument list
    [GeneratedRegex(@"(?<prefix>\bat\s+)(?:[\w\.`<>+\[\]]+\.)*(?:\.+)?(?<method>\w+)(?<args>\([^\)]*\))")]
    private static partial Regex MethodSanitizerRegex();


}
