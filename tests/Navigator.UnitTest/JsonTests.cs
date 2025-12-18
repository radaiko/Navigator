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
        foreach (var component in j["components"].A) {
            // Just verify we can iterate and access nested objects
            Assert.NotNull(component.O);
        }
    }

    // ============ Edge cases and parsing tests ============

    [Fact]
    public void Parse_Whitespace_Is_Handled_Correctly() {
        var txt = "  {  \"a\"  :  \"x\"  ,  \"b\"  :  123  }  ";
        var j = new Json(txt);
        Assert.Equal("x", j.GetString("a"));
        Assert.Equal(123L, j.GetLong("b"));
    }

    [Fact]
    public void Parse_Empty_Object_Works() {
        var j = new Json("{}");
        Assert.Empty(j.GetArray("any"));
        Assert.Equal(string.Empty, j.GetString("any"));
    }

    [Fact]
    public void Parse_Empty_String_Value_Works() {
        var j = new Json("{\"a\":\"\"}");
        Assert.Equal(string.Empty, j.GetString("a"));
    }

    [Fact]
    public void Parse_Strings_With_Escaped_Characters() {
        var txt = "{\"escaped\": \"line1\\nline2\\ttab\\\\\"}";
        var j = new Json(txt);
        var val = j.GetString("escaped");
        Assert.Contains("\n", val);
        Assert.Contains("\t", val);
        Assert.Contains("\\", val);
    }

    [Fact]
    public void Parse_Strings_With_Escaped_Quotes() {
        var txt = "{\"quoted\": \"He said \\\"Hello\\\"\"}";
        var j = new Json(txt);
        Assert.Equal("He said \"Hello\"", j.GetString("quoted"));
    }

    [Fact]
    public void Parse_Strings_With_Unicode_Escape_Sequences() {
        var txt = "{\"unicode\": \"\\u0041\\u0042\\u0043\"}";
        var j = new Json(txt);
        Assert.Equal("ABC", j.GetString("unicode"));
    }

    [Fact]
    public void Parse_Negative_Numbers() {
        var txt = "{\"n1\": -123, \"n2\": -3.14, \"n3\": -1e5}";
        var j = new Json(txt);
        Assert.Equal(-123L, j.GetLong("n1"));
        Assert.Equal(-3.14, j.GetDouble("n2"), 6);
        Assert.Equal(-100000.0, j.GetDouble("n3"), 6);
    }

    [Fact]
    public void Parse_Scientific_Notation_With_Plus_Sign() {
        var txt = "{\"n\": 1.5e+3}";
        var j = new Json(txt);
        Assert.Equal(1500.0, j.GetDouble("n"), 6);
    }

    [Fact]
    public void Parse_Scientific_Notation_With_Minus_Sign() {
        var txt = "{\"n\": 1.5e-3}";
        var j = new Json(txt);
        Assert.Equal(0.0015, j.GetDouble("n"), 6);
    }

    [Fact]
    public void Parse_Zero_And_Fractional_Numbers() {
        var txt = "{\"z\": 0, \"f\": 0.5, \"nz\": -0}";
        var j = new Json(txt);
        Assert.Equal(0L, j.GetLong("z"));
        Assert.Equal(0.5, j.GetDouble("f"), 6);
        Assert.Equal(0L, j.GetLong("nz"));
    }

    [Fact]
    public void Parse_Large_Numbers() {
        var txt = "{\"big\": 9223372036854775807}";
        var j = new Json(txt);
        Assert.Equal(long.MaxValue, j.GetLong("big"));
    }

    [Fact]
    public void Parse_Multiple_Array_Elements() {
        var txt = "{\"arr\": [1, \"two\", 3.0, true, false, null]}";
        var j = new Json(txt);
        var arr = j.GetArray("arr");
        Assert.Equal(6, arr.Count);
        Assert.Equal(1.0, arr[0].D);
        Assert.Equal("two", arr[1].S);
        Assert.Equal(3.0, arr[2].D);
        Assert.True(arr[3].B);
        Assert.False(arr[4].B);
        Assert.Empty(arr[5].S);
    }

    [Fact]
    public void Parse_Nested_Arrays() {
        var txt = "{\"nested\": [[1, 2], [3, 4]]}";
        var j = new Json(txt);
        var outer = j.GetArray("nested");
        Assert.Equal(2, outer.Count);
        var inner0 = outer[0].A;
        Assert.Equal(2, inner0.Count);
        Assert.Equal(1.0, inner0[0].D);
        Assert.Equal(2.0, inner0[1].D);
    }

    [Fact]
    public void Parse_Invalid_Json_Throws() {
        Assert.Throws<FormatException>(() => new Json("{\"a\": "));
        Assert.Throws<FormatException>(() => new Json("{invalid}"));
        Assert.Throws<FormatException>(() => new Json("{\"a\": 1,}"));
    }

    [Fact]
    public void Parse_Unterminated_String_Throws() {
        Assert.Throws<FormatException>(() => new Json("{\"unterminated"));
    }

    [Fact]
    public void Parse_Missing_Colon_In_Object_Throws() {
        Assert.Throws<FormatException>(() => new Json("{\"key\" \"value\"}"));
    }

    [Fact]
    public void Parse_Invalid_Number_Throws() {
        Assert.Throws<FormatException>(() => new Json("{\"n\": 123abc}"));
    }

    [Fact]
    public void Parse_Trailing_Comma_In_Array_Throws() {
        Assert.Throws<FormatException>(() => new Json("{\"arr\": [1, 2,]}"));
    }

    // ============ Serialization tests ============

    [Fact]
    public void ToJsonString_Produces_Compact_Output() {
        var j = new Json("{\"a\":\"x\",\"b\":123}");
        var json = j.ToJsonString();
        Assert.DoesNotContain("\n", json);
        Assert.Contains("\"a\":\"x\"", json);
        Assert.Contains("\"b\":123", json);
    }

    [Fact]
    public void ToJsonString_Produces_Pretty_Output() {
        var j = new Json("{\"a\":\"x\",\"b\":123}");
        var json = j.ToJsonString(pretty: true);
        Assert.Contains("\n", json);
        Assert.Contains("  ", json); // indentation
    }

    [Fact]
    public void ToJsonString_Pretty_Has_Proper_Indentation() {
        var j = new Json("{\"a\":{\"b\":\"value\"}}");
        var json = j.ToJsonString(pretty: true);
        Assert.Contains("  \"a\"", json);
        Assert.Contains("    \"b\"", json);
    }

    [Fact]
    public void ToJsonString_Escapes_Special_Characters() {
        var txt = "{\"text\": \"line1\\nline2\\t\\\"quoted\\\"\"}";
        var j = new Json(txt);
        var json = j.ToJsonString();
        var j2 = new Json(json);
        var val = j2.GetString("text");
        Assert.Contains("\n", val);
        Assert.Contains("\t", val);
        Assert.Contains("\"", val);
    }

    [Fact]
    public void ToJsonString_Escapes_Backslashes() {
        var j = new Json("{\"path\": \"C:\\\\Users\\\\test\"}");
        var json = j.ToJsonString();
        Assert.Contains("\\\\", json);
    }

    [Fact]
    public void ToJsonString_Serializes_All_Primitive_Types() {
        var txt = "{\"str\": \"hello\", \"num\": 42, \"dec\": 3.14, \"bool_t\": true, \"bool_f\": false, \"null_val\": null}";
        var j = new Json(txt);
        var json = j.ToJsonString();
        Assert.Contains("\"str\":\"hello\"", json);
        Assert.Contains("\"num\":42", json);
        Assert.Contains("\"dec\":3.14", json);
        Assert.Contains("\"bool_t\":true", json);
        Assert.Contains("\"bool_f\":false", json);
        Assert.Contains("\"null_val\":null", json);
    }

    [Fact]
    public void ToJsonString_Preserves_Roundtrip_Fidelity() {
        var original = "{\"a\":{\"b\":[1,2,3]},\"c\":null}";
        var j = new Json(original);
        var serialized = j.ToJsonString();
        var reparsed = new Json(serialized);
        Assert.Equal("", reparsed.GetString("c"));
        Assert.Equal(1.0, reparsed["a"]["b"][0].D);
    }

    [Fact]
    public void ToJsonString_Roundtrip_Preserves_Nested_Objects() {
        var original = "{\"a\":{\"b\":{\"c\":\"value\"}}}";
        var j = new Json(original);
        Assert.Equal("value", j.GetString("a.b.c"));
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal("value", j2.GetString("a.b.c"));
    }

    [Fact]
    public void ToJsonString_Roundtrip_With_Multiple_Values() {
        var original = "{\"a\": \"old\"}";
        var j = new Json(original);
        Assert.Equal("old", j.GetString("a"));
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal("old", j2.GetString("a"));
    }

    [Fact]
    public void ToJsonString_Roundtrip_Nested_Updates() {
        var original = "{\"a\": {\"b\": \"old\"}}";
        var j = new Json(original);
        Assert.Equal("old", j.GetString("a.b"));
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal("old", j2.GetString("a.b"));
    }

    [Fact]
    public void ToJsonString_Roundtrip_Numeric_Values() {
        var j = new Json("{\"a\": 42, \"b\": 3.14}");
        Assert.Equal(42L, j.GetLong("a"));
        Assert.Equal(3.14, j.GetDouble("b"), 6);
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal(42L, j2.GetLong("a"));
        Assert.Equal(3.14, j2.GetDouble("b"), 6);
    }

    [Fact]
    public void ToJsonString_Roundtrip_Boolean_Values() {
        var j = new Json("{\"t\": true, \"f\": false}");
        Assert.True(j.GetBool("t"));
        Assert.False(j.GetBool("f"));
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.True(j2.GetBool("t"));
        Assert.False(j2.GetBool("f"));
    }

    [Fact]
    public void ToJsonString_Roundtrip_Null_Values() {
        var txt = "{\"nullable\": null}";
        var j = new Json(txt);
        Assert.Equal(string.Empty, j.GetString("nullable"));
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal(string.Empty, j2.GetString("nullable"));
    }

    [Fact]
    public void ToString_Returns_Compact_Json() {
        var j = new Json("{\"a\":1}");
        var str = j.ToString();
        Assert.DoesNotContain("\n", str);
        Assert.Contains("\"a\":1", str);
    }

    // ============ Type conversion and accessors ============

    [Fact]
    public void TryGet_Converts_Between_Numeric_Types() {
        var j = new Json("{\"n\": 42}");
        Assert.True(j.TryGet<long>("n", out var l));
        Assert.Equal(42L, l);
        Assert.True(j.TryGet<int>("n", out var i));
        Assert.Equal(42, i);
        Assert.True(j.TryGet<double>("n", out var d));
        Assert.Equal(42.0, d, 6);
    }

    [Fact]
    public void TryGet_Returns_False_For_Missing_Key() {
        var j = new Json("{}");
        Assert.False(j.TryGet<string>("missing", out var _));
        Assert.False(j.TryGet<long>("missing", out var _));
    }

    [Fact]
    public void TryGet_Converts_Numbers_To_Strings() {
        var j = new Json("{\"n\": 42}");
        Assert.True(j.TryGet<string>("n", out var s));
        Assert.Equal("42", s);
    }

    [Fact]
    public void GetLong_Returns_Default_For_Missing_Key() {
        var j = new Json("{}");
        Assert.Equal(0L, j.GetLong("missing"));
        Assert.Equal(999L, j.GetLong("missing", 999L));
    }

    [Fact]
    public void GetLong_Converts_Double_To_Long() {
        var j = new Json("{\"n\": 42.0}");
        Assert.Equal(42L, j.GetLong("n"));
    }

    [Fact]
    public void GetLong_Converts_String_To_Long() {
        var j = new Json("{\"n\": \"42\"}");
        Assert.Equal(42L, j.GetLong("n"));
    }

    [Fact]
    public void GetLong_Handles_Negative_Conversion() {
        var j = new Json("{\"n\": \"-42\"}");
        Assert.Equal(-42L, j.GetLong("n"));
    }

    [Fact]
    public void GetDouble_Returns_NaN_For_Non_Numeric() {
        var j = new Json("{\"s\": \"text\"}");
        Assert.True(double.IsNaN(j.GetDouble("s")));
    }

    [Fact]
    public void GetDouble_Returns_Default_For_Missing() {
        var j = new Json("{}");
        Assert.Equal(0.0, j.GetDouble("missing"));
        Assert.Equal(3.14, j.GetDouble("missing", 3.14), 6);
    }

    [Fact]
    public void GetBool_Returns_Default_For_Missing_Key() {
        var j = new Json("{}");
        Assert.False(j.GetBool("missing"));
        Assert.True(j.GetBool("missing", true));
    }

    [Fact]
    public void GetString_Returns_Default_For_Missing_Key() {
        var j = new Json("{}");
        Assert.Equal("", j.GetString("missing"));
        Assert.Equal("default", j.GetString("missing", "default"));
    }

    [Fact]
    public void JsonVal_Implicit_String_Conversion() {
        var j = new Json("{\"a\": \"hello\"}");
        string? val = j["a"];
        Assert.Equal("hello", val);
    }

    [Fact]
    public void JsonVal_OOr_Returns_Default_For_Null() {
        var j = new Json("{}");
        var def = new Json();
        var result = j["missing"].OOr(def);
        Assert.Same(def, result);
    }

    [Fact]
    public void JsonVal_AOr_Returns_Default_For_Null() {
        var j = new Json("{}");
        var result = j["missing"].AOr();
        Assert.NotNull(result);
    }

    [Fact]
    public void JsonVal_IndexerInt_Returns_Null_For_OutOfRange() {
        var j = new Json("{\"arr\": [1, 2]}");
        var val = j["arr"][10];
        Assert.True(double.IsNaN(val.D));
    }

    [Fact]
    public void JsonVal_IndexerInt_Works_For_Arrays() {
        var j = new Json("{\"arr\": [10, 20, 30]}");
        Assert.Equal(10.0, j["arr"][0].D);
        Assert.Equal(20.0, j["arr"][1].D);
        Assert.Equal(30.0, j["arr"][2].D);
    }

    [Fact]
    public void JsonVal_IndexerString_Works_For_Objects() {
        var j = new Json("{\"obj\": {\"a\": 1, \"b\": 2}}");
        Assert.Equal(1.0, j["obj"]["a"].D);
        Assert.Equal(2.0, j["obj"]["b"].D);
    }

    [Fact]
    public void JsonVal_S_Converts_All_Primitive_Types() {
        var j = new Json("{\"n\": 42, \"b\": true, \"d\": 3.14}");
        Assert.Equal("42", j["n"].S);
        Assert.Equal("True", j["b"].S);
        Assert.Equal("3.14", j["d"].S);
    }

    [Fact]
    public void JsonVal_S_Returns_Empty_For_Null() {
        var j = new Json("{\"n\": null}");
        Assert.Equal(string.Empty, j["n"].S);
    }

    [Fact]
    public void JsonVal_D_Returns_Double_For_Numbers() {
        var j = new Json("{\"n\": 42.5}");
        Assert.Equal(42.5, j["n"].D, 6);
    }

    [Fact]
    public void JsonVal_D_Returns_NaN_For_Non_Numbers() {
        var j = new Json("{\"s\": \"text\"}");
        Assert.True(double.IsNaN(j["s"].D));
    }

    [Fact]
    public void JsonVal_B_Returns_Boolean() {
        var j = new Json("{\"t\": true, \"f\": false}");
        Assert.True(j["t"].B);
        Assert.False(j["f"].B);
    }

    // ============ Complex nested structure tests ============

    [Fact]
    public void Complex_Deeply_Nested_Mixed_Structure() {
        var txt = @"{
            ""users"": [
                {""id"": 1, ""name"": ""Alice"", ""tags"": [""admin"", ""user""]},
                {""id"": 2, ""name"": ""Bob"", ""tags"": [""user""]}
            ],
            ""metadata"": {""version"": 1, ""timestamp"": null}
        }";
        var j = new Json(txt);

        var users = j.GetArray("users");
        Assert.Equal(2, users.Count);

        var alice = users[0].OOr();
        Assert.Equal(1.0, alice.GetDouble("id"));
        Assert.Equal("Alice", alice.GetString("name"));

        var aliceTags = alice.GetArray("tags");
        Assert.Equal(2, aliceTags.Count);
        Assert.Equal("admin", aliceTags[0].S);

        Assert.Equal(1.0, j.GetDouble("metadata.version"));
        Assert.Equal(string.Empty, j.GetString("metadata.timestamp"));
    }

    [Fact]
    public void Parse_Object_With_Many_Keys() {
        var txt = "{\"k1\":1,\"k2\":2,\"k3\":3,\"k4\":4,\"k5\":5,\"k6\":6,\"k7\":7,\"k8\":8,\"k9\":9,\"k10\":10}";
        var j = new Json(txt);
        for (int i = 1; i <= 10; i++) {
            Assert.Equal(i, j.GetLong($"k{i}"));
        }
    }

    [Fact]
    public void Complex_Array_Of_Objects_Access() {
        var txt = "{\"items\": [{\"id\": 1, \"name\": \"Item1\"}, {\"id\": 2, \"name\": \"Item2\"}]}";
        var j = new Json(txt);
        var items = j.GetArray("items");
        var secondItem = items[1].OOr();
        Assert.Equal(2.0, secondItem.GetDouble("id"));
        Assert.Equal("Item2", secondItem.GetString("name"));
    }

    [Fact]
    public void Dot_Path_Access_Works_With_Numbers() {
        var txt = "{\"arr\": [1, 2, 3]}";
        var j = new Json(txt);
        Assert.Equal(2.0, j.GetDouble("arr.1"));
    }

    // ============ File I/O tests ============

    [Fact]
    public void Load_Reads_Json_From_File() {
        var tempFile = Path.GetTempFileName();
        try {
            File.WriteAllText(tempFile, "{\"test\": \"value\"}");
            var j = Json.Load(tempFile);
            Assert.Equal("value", j.GetString("test"));
        } finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Write_Writes_Json_To_File() {
        var tempFile = Path.GetTempFileName();
        try {
            var j = new Json("{\"key\": \"value\"}");
            j.Write(tempFile);

            var content = File.ReadAllText(tempFile);
            Assert.Contains("\"key\"", content);
            Assert.Contains("\"value\"", content);

            var reloaded = Json.Load(tempFile);
            Assert.Equal("value", reloaded.GetString("key"));
        } finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Write_And_Load_Roundtrip() {
        var tempFile = Path.GetTempFileName();
        try {
            var original = new Json("{\"a\": {\"b\": [1, 2, 3]}}");
            original.Write(tempFile);
            var loaded = Json.Load(tempFile);
            Assert.Equal(2.0, loaded["a"]["b"][1].D);
        } finally {
            File.Delete(tempFile);
        }
    }

    // ============ Edge case: special values and bounds ============

    [Fact]
    public void Handles_Very_Long_Strings() {
        var longString = new string('a', 10000);
        var json = $"{{\"long\": \"{longString}\"}}";
        var j = new Json(json);
        Assert.Equal(longString, j.GetString("long"));
    }

    [Fact]
    public void Handles_Backslashes_In_Strings() {
        var j = new Json(@"{""path"": ""C:\\Users\\test\\file.txt""}");
        Assert.Equal(@"C:\Users\test\file.txt", j.GetString("path"));
    }

    [Fact]
    public void Handles_Mixed_Escape_Sequences() {
        var txt = "{\"text\": \"\\r\\n\\t\\\\\\\"\"}";
        var j = new Json(txt);
        var val = j.GetString("text");
        Assert.Contains("\r", val);
        Assert.Contains("\n", val);
        Assert.Contains("\t", val);
        Assert.Contains("\\", val);
        Assert.Contains("\"", val);
    }

    [Fact]
    public void Handles_Form_Feed_And_Backspace() {
        var txt = "{\"text\": \"\\f\\b\"}";
        var j = new Json(txt);
        var val = j.GetString("text");
        Assert.Contains("\f", val);
        Assert.Contains("\b", val);
    }

    [Fact]
    public void Empty_Constructor_Creates_Empty_Object() {
        var j = new Json();
        Assert.Empty(j.GetArray("nonexistent"));
        Assert.Equal(string.Empty, j.GetString("nonexistent"));
    }

    [Fact]
    public void GetObject_Returns_Empty_Object_For_Missing_Path() {
        var j = new Json("{}");
        var obj = j.GetObject("missing.path.here");
        Assert.Equal(string.Empty, obj.GetString("any"));
    }

    [Fact]
    public void Chained_Indexer_Access_Returns_Safe_Defaults() {
        var j = new Json("{}");
        var val = j["a"]["b"]["c"]["d"].S;
        Assert.Equal(string.Empty, val);
    }

    [Fact]
    public void Array_Index_On_Non_Array_Returns_Null() {
        var j = new Json("{\"notarray\": \"string\"}");
        var val = j["notarray"][0];
        Assert.True(double.IsNaN(val.D));
    }

    [Fact]
    public void Parse_False_Boolean() {
        var j = new Json("{\"b\": false}");
        Assert.False(j.GetBool("b"));
    }

    [Fact]
    public void Parse_Null_Value() {
        var j = new Json("{\"n\": null}");
        Assert.True(double.IsNaN(j["n"].D));
        Assert.False(j["n"].B);
        Assert.Equal(string.Empty, j["n"].S);
    }

    [Fact]
    public void Control_Character_Unicode_Escape() {
        var txt = "{\"ctrl\": \"\\u0001\\u001f\"}";
        var j = new Json(txt);
        var val = j.GetString("ctrl");
        Assert.Equal((char)0x0001, val[0]);
        Assert.Equal((char)0x001f, val[1]);
    }

    [Fact]
    public void Empty_Array_Serialization() {
        var j = new Json("{\"arr\": []}");
        var json = j.ToJsonString();
        Assert.Contains("\"arr\":[]", json);
    }

    [Fact]
    public void Empty_Object_Serialization() {
        var j = new Json("{\"obj\": {}}");
        var json = j.ToJsonString();
        Assert.Contains("\"obj\":{}", json);
    }

    [Fact]
    public void Whitespace_Between_Array_Elements() {
        var txt = "{\"arr\": [  1  ,  2  ,  3  ]}";
        var j = new Json(txt);
        var arr = j.GetArray("arr");
        Assert.Equal(3, arr.Count);
        Assert.Equal(1.0, arr[0].D);
        Assert.Equal(2.0, arr[1].D);
        Assert.Equal(3.0, arr[2].D);
    }

    [Fact]
    public void Multiple_Decimal_Points_Invalid() {
        Assert.Throws<FormatException>(() => new Json("{\"n\": 1.2.3}"));
    }

    [Fact]
    public void Negative_Sign_After_E_In_Scientific() {
        var j = new Json("{\"n\": 5e-2}");
        Assert.Equal(0.05, j.GetDouble("n"), 6);
    }

    [Fact]
    public void GetArray_Returns_Empty_List_For_Non_Array() {
        var j = new Json("{\"obj\": {\"key\": \"value\"}}");
        var arr = j.GetArray("obj");
        Assert.Empty(arr);
    }

    [Fact]
    public void Access_Nested_Null_In_Object() {
        var j = new Json("{\"a\": {\"b\": null}}");
        Assert.Equal(string.Empty, j.GetString("a.b"));
    }

    [Fact]
    public void Forward_Slash_Escape_Sequence() {
        var txt = "{\"url\": \"http:\\/\\/example.com\"}";
        var j = new Json(txt);
        Assert.Equal("http://example.com", j.GetString("url"));
    }

    [Fact]
    public void Serialization_Preserves_Number_Precision() {
        var j = new Json("{\"pi\": 3.141592653589793}");
        var json = j.ToJsonString();
        var j2 = new Json(json);
        Assert.Equal(3.141592653589793, j2.GetDouble("pi"), 15);
    }

    [Fact]
    public void Multiple_Objects_In_Array() {
        var txt = "{\"objs\": [{\"a\": 1}, {\"b\": 2}, {\"c\": 3}]}";
        var j = new Json(txt);
        var objs = j.GetArray("objs");
        Assert.Equal(3, objs.Count);
        Assert.Equal(1.0, objs[0].OOr().GetDouble("a"));
        Assert.Equal(2.0, objs[1].OOr().GetDouble("b"));
        Assert.Equal(3.0, objs[2].OOr().GetDouble("c"));
    }

    [Fact]
    public void Test_O_Property_Returns_Json_Object() {
        var j = new Json("{\"nested\": {\"key\": \"value\"}}");
        var nested = j["nested"].O;
        Assert.NotNull(nested);
        Assert.Equal("value", nested.GetString("key"));
    }

    [Fact]
    public void Test_A_Property_Returns_JsonVal_List() {
        var j = new Json("{\"arr\": [1, 2, 3]}");
        var arr = j["arr"].A;
        Assert.Equal(3, arr.Count);
    }

    [Fact]
    public void Deeply_Nested_Dot_Path_Access() {
        var j = new Json("{\"a\": {\"b\": {\"c\": {\"d\": {\"e\": \"found\"}}}}}");
        Assert.Equal("found", j.GetString("a.b.c.d.e"));
    }

    [Fact]
    public void Parse_Array_With_Whitespace() {
        var txt = "{ \"arr\" : [ 1 , 2 , 3 ] }";
        var j = new Json(txt);
        var arr = j.GetArray("arr");
        Assert.Equal(3, arr.Count);
    }
}

