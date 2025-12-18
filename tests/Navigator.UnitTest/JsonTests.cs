using System.Text.Json;
using Navigator.UI.Models;
using Xunit;
using Navigator.UI.Utils;

namespace Navigator.UnitTest;

public class JsonTests {
    [Fact]
    public void Parse_Primitives_And_Accessors_Work() {
        var txt = "{\"a\":\"x\",\"b\":123,\"c\":1.5,\"d\":true,\"e\":null,\"arr\":[1,2,\"z\"],\"obj\":{\"x\":\"y\"}}";
        var j = new Json(txt);

        // string
        Assert.Equal("x", j.GetString("a"));
        Assert.Equal("x", j["a"].S);

        // integer (stored as long)
        Assert.Equal(123L, j.GetLong("b"));
        Assert.True(j.TryGet<long>("b", out var bl) && bl == 123L);

        // double
        Assert.Equal(1.5, j["c"].D, 6);
        Assert.True(j.TryGet<double>("c", out var cd) && Math.Abs(cd - 1.5) < 1e-9);

        // boolean
        Assert.True(j.GetBool("d"));
        Assert.True(j["d"].B);

        // null -> JsonVal.S is empty string, D is NaN, B is false
        Assert.Equal(string.Empty, j["e"].S);
        Assert.True(double.IsNaN(j["missing"].D)); // missing path -> JsonVal.D == NaN
        Assert.False(j["missing"].B);

        // GetDouble/GetString/GetBool return supplied default when path missing
        Assert.Equal(0.0, j.GetDouble("missing"));
        Assert.Equal("", j.GetString("missing"));
        Assert.False(j.GetBool("missing"));
    }

    [Fact]
    public void Parse_Arrays_And_Objects_Work() {
        var txt = @"{""arr"": [10, {""k"": ""v""}], ""empty"": []}";
        var j = new Json(txt);

        var arr = j.GetArray("arr");
        Assert.Equal(2, arr.Count);
        Assert.Equal(10.0, arr[0].D); // number as JsonVal.D

        var inner = arr[1].OOr();
        Assert.Equal("v", inner.GetString("k"));

        // empty array and missing array
        Assert.Empty(j.GetArray("empty"));
        Assert.Empty(j.GetArray("notthere"));

        // object access
        var obj = j.GetObject("arr.1");
        Assert.Equal("v", obj.GetString("k"));

        var missingObj = j.GetObject("no.such.object");
        Assert.Equal(string.Empty, missingObj.GetString("any"));
    }

    [Fact]
    public void Constructor_Throws_On_NonObject_Root() {
        var arrJson = "[1,2,3]";
        Assert.Throws<ArgumentException>(() => new Json(arrJson));
    }

    [Fact]
    public void Numeric_Formats_Are_Parsed_Correctly() {
        var txt = "{\"n1\": -42, \"n2\": 3.1415, \"n3\": 1e3}";
        var j = new Json(txt);
        Assert.Equal(-42L, j.GetLong("n1"));
        Assert.Equal(3.1415, j["n2"].D, 6);
        Assert.Equal(1000.0, j["n3"].D, 6);
    }

    [Fact]
    public void Nested_Objects_And_Arrays_Are_Handled_Correctly() {
        var txt = @"{""level1"": { ""level2"": { ""level3"": [ {""key"": ""value""} ] } } }";
        var j = new Json(txt);
        var level3Array = j.GetArray("level1.level2.level3");
        Assert.Single(level3Array);
        var level3Obj = level3Array[0].OOr();
        Assert.Equal("value", level3Obj.GetString("key"));
        Assert.Equal("value", j["level1"]["level2"]["level3"][0]["key"].S);
    }

    [Fact]
    public void TestSbom() {
        var sbomJson = "JsonTests/sbom.json".GetTestFileContent();
        var j = new Json(sbomJson);
        List<PackageInfo> packages = new();
        foreach (var component in j["components"].A) {
            packages.Add(new PackageInfo(component.O!));
        }

        Assert.EqualWithJson("JsonTests/packageInfo.json", packages);
    }
}
