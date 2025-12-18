using System.Text.Json;

namespace Navigator.UnitTest;

public static class TestExtensions {
    extension(string path) {
        public string GetTestFilePath() {
            var basePath = "../TestData/";
            var fullPath = Path.GetFullPath(Path.Combine(basePath, path));
            return fullPath;
        }

        public string GetTestFileContent() => File.ReadAllText(GetTestFilePath(path));
    }

    static JsonSerializerOptions? _testOptions;
    extension(JsonSerializerOptions) {
        public static JsonSerializerOptions WithTestOptions() => _testOptions ??= new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

}

public static class AssertExtensions {
    extension(Assert) {
        public static void EqualWithFile(string expectedFilePath, string actualContent) {
            expectedFilePath = expectedFilePath.GetTestFilePath();
            if (!File.Exists(expectedFilePath)) File.WriteAllText(expectedFilePath, actualContent);
            var expectedContent = expectedFilePath.GetTestFileContent();
            Assert.Equal(expectedContent, actualContent);
        }

        public static void EqualWithJson(string expectedJsonFilePath, object actualJson) {
            expectedJsonFilePath = expectedJsonFilePath.GetTestFilePath();
            var actualJsonString = JsonSerializer.Serialize(actualJson, JsonSerializerOptions.WithTestOptions());
            if (!File.Exists(expectedJsonFilePath))
                File.WriteAllText(expectedJsonFilePath, actualJsonString);
            var expectedContent = expectedJsonFilePath.GetTestFileContent();
            Assert.Equal(expectedContent, actualJsonString);
        }
    }
}
