using System.Collections.Generic;

namespace Navigator.UI.Models;

public class History {
    private int _index = -1;
    private readonly List<string> _paths = [];

    public void Add(string path) {
        if (_index >= 0 && _index < _paths.Count && _paths[_index] == path) {
            LogDebug($"Skipping duplicate history entry for: {path}");
            return;
        }

        if (_index < _paths.Count - 1) {
            int removed = _paths.Count - (_index + 1);
            _paths.RemoveRange(_index + 1, removed);
            LogDebug($"Trimmed {removed} forward entries from history");
        }

        _paths.Add(path);
        _index = _paths.Count - 1;
        LogDebug($"Added path to history: {path} at index {_index}");
    }

    public string? TryGetPrevious() {
        if (!CanGoToPrevious())
            return LogAndReturnNull("Cannot navigate to previous path: already at the beginning");

        _index--;
        var path = _paths[_index];
        LogDebug($"Navigated to previous path: {path} at index {_index}");
        return path;
    }

    public string? TryGetNext() {
        if (!CanGoToNext())
            return LogAndReturnNull("Cannot navigate to next path: already at the end");

        _index++;
        var path = _paths[_index];
        LogDebug($"Navigated to next path: {path} at index {_index}");
        return path;
    }

    private bool CanGoToPrevious() => _index > 0;

    private bool CanGoToNext() => _index < _paths.Count - 1;

    private static void LogDebug(string message) => Logger.Debug(message);

    private static string? LogAndReturnNull(string message) {
        LogDebug(message);
        return null;
    }
}

