using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.Converters {
    public class DirectoryNodeFilterConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
            if (value is not System.Collections.IEnumerable children) return new List<DirectoryNode>();
            return children.Cast<object>().OfType<DirectoryNode>().ToList();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
            throw new System.NotImplementedException();
        }
    }
}
