using System.Globalization;
using System.Text;

namespace Navigator.UI.Utils;

/// <summary>
/// A tiny, fast JSON utility representing a JSON object as a lightweight in-memory structure.
/// Supports parsing from text, basic dot-path access, and writing back to compact or pretty JSON.
/// Designed for high-performance and minimal dependencies for simple read/write scenarios.
/// </summary>
public sealed class Json {
    private readonly Dictionary<string, object?> _map;

    /// <summary>
    /// Lightweight reference wrapper returned by the indexer. Can be null.
    /// Provides typed accessors (S/I/D/B) and implicit conversion to string for convenient usage.
    /// </summary>
    public readonly struct JsonVal {
        internal readonly object? V;
        internal JsonVal(object? v) { V = v; }

        /// <summary>Raw value as string if the underlying value is a string, or a culture-invariant representation of primitives; otherwise empty string.</summary>
        public string S {
            get {
                if (V == null) return string.Empty;
                if (V is string s) return s;
                if (V is bool || V is long || V is int || V is double || V is float || V is decimal)
                    return Convert.ToString(V, CultureInfo.InvariantCulture) ?? string.Empty;
                return string.Empty;
            }
        }

        /// <summary>Floating point value when underlying value is numeric.</summary>
        public double D {
            get {
                if (V is double d) return d;
                if (V is float f) return f;
                if (V is long l) return l;
                if (V is int i) return i;
                return double.NaN;
            }
         }

        /// <summary>Boolean value when underlying value is boolean; returns false when missing.</summary>
        public bool B => V is bool b ? b : false;

         /// <summary>Returns the value as a <see cref="Json"/> object if it is an object.</summary>
         public Json? O => V as Json;

         /// <summary>Returns the value as an array of <see cref="JsonVal"/> if underlying is an array.</summary>
         public IList<JsonVal> A {
             get {
                 if (V is IList<object?> arr) {
                     var list = new List<JsonVal>(arr.Count);
                     foreach (var x in arr) list.Add(new JsonVal(x));
                     return list;
                 }
                 return [];
             }
         }

         /// <summary>Implicit conversion to string (returns S).</summary>
         public static implicit operator string?(JsonVal? v) => v?.S;

         /// <summary>String representation (falls back to raw ToString).</summary>
         public override string ToString() => S;

        // Convenience helpers for objects/arrays. S/D/B already have safe defaults.
        public Json OOr(Json? defaultValue = null) => O ?? defaultValue ?? new Json();
        public IList<JsonVal> AOr(IList<JsonVal>? defaultValue = null) => A ?? defaultValue ?? new List<JsonVal>();

         /// <summary>Safe indexer for array-like values. Returns a JsonVal wrapper (may contain null) when index out of range or not an array.</summary>
         public JsonVal this[int index] {
             get {
                 if (V is IList<object?> arr) {
                     if (index >= 0 && index < arr.Count) return new JsonVal(arr[index]);
                 }
                 return new JsonVal(null);
             }
         }

         /// <summary>Safe string indexer for object-like values (supports chaining: j["a"]["b"]). Returns a JsonVal wrapper (may contain null) when key missing or not an object.</summary>
         public JsonVal this[string key] {
             get {
                 if (V is Json jo) {
                    if (jo._map.TryGetValue(key, out var o)) return new JsonVal(o);
                    return new JsonVal(null);
                 }
                 if (V is IDictionary<string, object?> dict) {
                    if (dict.TryGetValue(key, out var o)) return new JsonVal(o);
                    return new JsonVal(null);
                 }
                 return new JsonVal(null);
             }
         }
     }

    /// <summary>
    /// Creates a new empty JSON object.
    /// Use the indexer or <see cref="SetByPath(string, object?)"/> to populate values.
    /// </summary>
    public Json() {
        _map = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Parses a JSON document from the supplied <paramref name="jsonText"/> and
    /// constructs a <see cref="Json"/> representing the root object.
    /// Throws <see cref="ArgumentException"/> if the root value is not a JSON object.
    /// This parser supports objects, arrays, strings, numbers, booleans and null.
    /// </summary>
    /// <param name="jsonText">The JSON text to parse.</param>
    public Json(string jsonText) : this() {
        var p = new Parser(jsonText.AsSpan());
        var v = p.ParseValue();
        if (v is Json j) {
            foreach (var kv in j._map) _map[kv.Key] = kv.Value;
        } else {
            throw new ArgumentException("Root JSON value must be an object", nameof(jsonText));
        }
    }

    /// <summary>
    /// Loads and parses a JSON file from disk and returns the parsed <see cref="Json"/> object.
    /// </summary>
    /// <param name="path">Path to the JSON file.</param>
    /// <returns>Parsed <see cref="Json"/> representing the root object.</returns>
    public static Json Load(string path) {
        var txt = File.ReadAllText(path);
        return new Json(txt);
    }

    /// <summary>
    /// Writes the current JSON object to a file at <paramref name="path"/> in compact form.
    /// </summary>
    /// <param name="path">Target file path.</param>
    public void Write(string path) => File.WriteAllText(path, ToJsonString());

    // Indexer: supports dot-separated nested access (e.g. "a.b.c")
    /// <summary>
    /// Gets or sets a value by a dot-separated path (e.g. "parent.child.name").
    /// Getting returns a lightweight <see cref="JsonVal"/> wrapper (never null). Setting will create intermediate objects if necessary.
    /// </summary>
    /// <param name="key">Dot-separated path to the value.</param>
    public JsonVal this[string key] {
        get {
            var o = GetByPathObj(key);
            return new JsonVal(o);
        }
        set => SetByPath(key, value.V);
    }

    /// <summary>
    /// Attempts to retrieve a value at the given dot-separated path and convert it to <typeparamref name="T"/>.
    /// Returns true when the value exists and is convertible to the requested type.
    /// </summary>
    /// <typeparam name="T">Requested target type.</typeparam>
    /// <param name="key">Dot-separated path.</param>
    /// <param name="value">Out parameter set to the converted value on success, or default on failure.</param>
    /// <returns>True if the value exists and was converted to <typeparamref name="T"/>; otherwise false.</returns>
    public bool TryGet<T>(string key, out T? value) {
        var v = GetByPathObj(key);
        if (v is T t) {
            value = t;
            return true;
        }
        // try to convert primitives
        try {
            if (v == null) {
                value = default;
                return false;
            }
            var converted = (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
            value = converted;
            return true;
        } catch {
            value = default;
            return false;
        }
    }

    // Convenience getters that return default values when the path is missing or null.
    public string GetString(string path, string defaultValue = "") {
        var v = GetByPathObj(path);
        if (v == null) return defaultValue;
        var s = this[path].S;
        return string.IsNullOrEmpty(s) ? defaultValue : s;
    }

    public long GetLong(string path, long defaultValue = 0) {
        var v = GetByPathObj(path);
        if (v == null) return defaultValue;
        if (v is long l) return l;
        if (v is int i) return i;
        if (v is double d && Math.Abs(Math.Floor(d) - d) < 0.001) return (long)d;
        if (v is string s && long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)) return parsed;
        return defaultValue;
    }

    public double GetDouble(string path, double defaultValue = 0.0) {
        var v = GetByPathObj(path);
        if (v == null) return defaultValue;
        return this[path].D; // returns NaN if value isn't numeric
    }

    public bool GetBool(string path, bool defaultValue = false) {
        var v = GetByPathObj(path);
        if (v == null) return defaultValue;
        return this[path].B; // returns false when underlying value is missing or not boolean
    }

    public Json GetObject(string path) => this[path].OOr();
    public IList<JsonVal> GetArray(string path) => this[path].AOr();

    // Internal helper returns raw stored object at path (or null)
    private object? GetByPathObj(string path) {
        if (string.IsNullOrEmpty(path)) return null;
        var parts = path.Split('.');
        object? cur = this;
        foreach (var p in parts) {
            if (cur is Json j) {
                if (!j._map.TryGetValue(p, out cur)) return null;
            } else if (cur is IDictionary<string, object?> dict) {
                if (!dict.TryGetValue(p, out cur)) return null;
            } else if (cur is IList<object?> list) {
                // numeric index access for arrays (e.g. "arr.0")
                if (int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx)) {
                    if (idx < 0 || idx >= list.Count) return null;
                    cur = list[idx];
                } else return null;
            } else {
                return null;
            }
        }
        return cur;
    }

    private void SetByPath(string path, object? value) {
        if (string.IsNullOrEmpty(path)) return;
        var parts = path.Split('.');
        Json cur = this;
        for (int i = 0; i < parts.Length - 1; i++) {
            var p = parts[i];
            if (cur._map.TryGetValue(p, out var next)) {
                if (next is Json nj) {
                    cur = nj;
                } else if (next is Dictionary<string, object?> d) {
                    var wrap = new Json();
                    foreach (var kv in d) wrap._map[kv.Key] = kv.Value;
                    cur._map[p] = wrap;
                    cur = wrap;
                } else {
                    var wrap = new Json();
                    cur._map[p] = wrap;
                    cur = wrap;
                }
            } else {
                var n = new Json();
                cur._map[p] = n;
                cur = n;
            }
        }
        cur._map[parts[^1]] = value;
    }

    /// <summary>
    /// Returns compact JSON text representing this object. Equivalent to <see cref="ToJsonString(bool)"/> with pretty=false.
    /// </summary>
    public override string ToString() => ToJsonString();

    /// <summary>
    /// Serializes the JSON object to a string.
    /// Set <paramref name="pretty"/> to true to produce indented, human-friendly output.
    /// </summary>
    /// <param name="pretty">When true, produce pretty/indented JSON; otherwise produce compact JSON.</param>
    /// <returns>Serialized JSON string.</returns>
    public string ToJsonString(bool pretty = false) {
        var sb = new StringBuilder(256);
        WriteValue(this, sb, pretty ? 0 : null);
        return sb.ToString();
    }

    private static void WriteValue(object? v, StringBuilder sb, int? indent) {
        if (v == null) { sb.Append("null"); return; }
        if (v is Json j) {
            WriteObject(j._map, sb, indent);
            return;
        }
        if (v is IDictionary<string, object?> dict) {
            WriteObject(dict, sb, indent);
            return;
        }
        if (v is IList<object?> list) {
            WriteArray(list, sb, indent);
            return;
        }
        if (v is string s) {
            sb.Append('"');
            EscapeString(s, sb);
            sb.Append('"');
            return;
        }
        if (v is bool b) { sb.Append(b ? "true" : "false"); return; }
        if (v is double || v is float || v is decimal) {
            sb.Append(Convert.ToString(v, CultureInfo.InvariantCulture));
            return;
        }
        if (v is int || v is long || v is short || v is byte || v is uint || v is ulong) {
            sb.Append(Convert.ToString(v, CultureInfo.InvariantCulture));
            return;
        }
        // Fallback to string
        sb.Append('"');
        EscapeString(v.ToString() ?? string.Empty, sb);
        sb.Append('"');
    }

    private static void WriteObject(IDictionary<string, object?> dict, StringBuilder sb, int? indent) {
        var first = true;
        sb.Append('{');
        if (indent != null) sb.Append('\n');
        if (indent != null) {
            int nextIndent = indent.Value + 2;
            foreach (var kv in dict) {
                if (!first) {
                    sb.Append(',');
                    sb.Append('\n');
                }
                first = false;
                sb.Append(' ', nextIndent);
                sb.Append('"');
                EscapeString(kv.Key, sb);
                sb.Append('"');
                sb.Append(':');
                sb.Append(' ');
                WriteValue(kv.Value, sb, nextIndent);
            }
            sb.Append('\n');
            sb.Append(' ', indent.Value);
        } else {
            foreach (var kv in dict) {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"');
                EscapeString(kv.Key, sb);
                sb.Append('"');
                sb.Append(':');
                WriteValue(kv.Value, sb, null);
            }
        }
        sb.Append('}');
    }

    private static void WriteArray(IList<object?> arr, StringBuilder sb, int? indent) {
        sb.Append('[');
        if (arr.Count == 0) { sb.Append(']'); return; }
        if (indent != null) {
            sb.Append('\n');
            int nextIndent = indent.Value + 2;
            for (int i = 0; i < arr.Count; i++) {
                if (i > 0) { sb.Append(','); sb.Append('\n'); }
                sb.Append(' ', nextIndent);
                WriteValue(arr[i], sb, nextIndent);
            }
            sb.Append('\n');
            sb.Append(' ', indent.Value);
        } else {
            for (int i = 0; i < arr.Count; i++) {
                if (i > 0) sb.Append(',');
                WriteValue(arr[i], sb, null);
            }
        }
        sb.Append(']');
    }

    private static void EscapeString(string s, StringBuilder sb) {
        foreach (var c in s) {
            switch (c) {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (char.IsControl(c)) {
                        sb.Append('\\');
                        sb.Append('u');
                        sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    } else sb.Append(c);
                    break;
            }
        }
    }

    // Parser: fast recursive descent over a string
    private ref struct Parser {
        private ReadOnlySpan<char> _s;
        private int _i;

        public Parser(ReadOnlySpan<char> s) {
            _s = s;
            _i = 0;
        }

        public object? ParseValue() {
            SkipWhitespace();
            if (_i >= _s.Length) return null;
            switch (_s[_i]) {
                case '{': return ParseObject();
                case '[': return ParseArray();
                case '"': return ParseString();
                case 't': return ParseLiteral("true", true);
                case 'f': return ParseLiteral("false", false);
                case 'n': return ParseLiteral("null", null);
                default:
                    return ParseNumberOrUnquoted();
            }
        }

        private object? ParseLiteral(string literal, object? value) {
            if (_i + literal.Length <= _s.Length && _s.Slice(_i, literal.Length).SequenceEqual(literal.AsSpan())) {
                _i += literal.Length;
                return value;
            }
            throw new FormatException($"Invalid token at {_i}");
        }

        private object? ParseNumberOrUnquoted() {
            var start = _i;
            if (_i < _s.Length && _s[_i] == '-' ) _i++;
            while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
            var isFloat = false;
            if (_i < _s.Length && _s[_i] == '.') {
                isFloat = true; _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
            }
            if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E')) {
                isFloat = true; _i++;
                if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
            }

            var slice = _s.Slice(start, _i - start);
            if (slice.Length == 0) throw new FormatException($"Unexpected character '{_s[_i]}' at {_i}");

            if (isFloat) {
                if (double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return d;
            } else {
                if (long.TryParse(slice, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return l;
            }
            // fallback
            if (double.TryParse(slice, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var fd)) return fd;
            throw new FormatException($"Invalid number '{slice.ToString()}'");
        }

        private Json ParseObject() {
            // assume current is '{'
            _i++; SkipWhitespace();
            var obj = new Json();
            if (_i < _s.Length && _s[_i] == '}') { _i++; return obj; }
            while (true) {
                SkipWhitespace();
                if (_i >= _s.Length || _s[_i] != '"') throw new FormatException("Expected string for object key");
                var key = ParseString();
                SkipWhitespace();
                if (_i >= _s.Length || _s[_i] != ':') throw new FormatException("Expected ':' after object key");
                _i++; SkipWhitespace();
                var value = ParseValue();
                obj._map[key] = value;
                SkipWhitespace();
                if (_i >= _s.Length) throw new FormatException("Unterminated object");
                if (_s[_i] == '}') { _i++; break; }
                if (_s[_i] == ',') { _i++; continue; }
                throw new FormatException($"Unexpected character '{_s[_i]}' in object at {_i}");
            }
            return obj;
        }

        private IList<object?> ParseArray() {
            _i++; SkipWhitespace();
            var list = new List<object?>();
            if (_i < _s.Length && _s[_i] == ']') { _i++; return list; }
            while (true) {
                SkipWhitespace();
                var v = ParseValue();
                list.Add(v);
                SkipWhitespace();
                if (_i >= _s.Length) throw new FormatException("Unterminated array");
                if (_s[_i] == ']') { _i++; break; }
                if (_s[_i] == ',') { _i++; continue; }
                throw new FormatException($"Unexpected character '{_s[_i]}' in array at {_i}");
            }
            return list;
        }

        private string ParseString() {
            if (_s[_i] != '"') throw new FormatException("Expected string");
            _i++;
            var start = _i;
            while (_i < _s.Length) {
                var c = _s[_i];
                if (c == '"') {
                    var res = _s.Slice(start, _i - start).ToString();
                    _i++;
                    return res;
                }
                if (c == '\\') {
                    return ParseStringSlow(start);
                }
                _i++;
            }
            throw new FormatException("Unterminated string");
        }

        private string ParseStringSlow(int start) {
            var sb = new StringBuilder();
            sb.Append(_s.Slice(start, _i - start));
            while (_i < _s.Length) {
                var c = _s[_i++];
                if (c == '"') return sb.ToString();
                if (c == '\\') {
                    if (_i >= _s.Length) break;
                    var esc = _s[_i++];
                    switch (esc) {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u': {
                            if (_i + 4 > _s.Length) throw new FormatException("Invalid unicode escape");
                            var hex = _s.Slice(_i, 4);
                            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code)) throw new FormatException("Invalid unicode escape");
                            sb.Append((char)code);
                            _i += 4;
                            break;
                        }
                        default:
                            sb.Append(esc); break;
                    }
                } else {
                    sb.Append(c);
                }
            }
            throw new FormatException("Unterminated string");
        }

        private void SkipWhitespace() {
            while (_i < _s.Length) {
                var c = _s[_i];
                if (c == ' ' || c == '\n' || c == '\r' || c == '\t') { _i++; continue; }
                break;
            }
        }
    }
}
