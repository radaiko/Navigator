using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Navigator.Models.Nodes;

namespace Navigator.Converters;

public class DirectoryNodeFilterConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is IEnumerable<BaseNode> children) {
            return children.OfType<DirectoryNode>().ToList();
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
