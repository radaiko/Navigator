namespace Navigator.UnitTest;

public static class TestExtensions {
    public static string GetTestFileContent(string path) {
        var basePath = "../TestData/";
        var fullPath = Path.GetFullPath(Path.Combine(basePath, path));
        return File.ReadAllText(fullPath);
    }
}
