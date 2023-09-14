using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;
using System.Linq;

namespace AppsettingsProtector;

public class JsonConfigurationDictionaryParser
{
    public static JsonConfigurationDictionaryParser GetNewInstance() => new();

    protected JsonConfigurationDictionaryParser() { }

    private readonly IDictionary<string, string?> _data =
        new SortedDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    private readonly Stack<string> _context = new();
    private string? _currentPath;

    public static IDictionary<string, string?> Parse(string input)
    {
        return GetNewInstance().ParseString(input);
    }

    public static IDictionary<string, string?> Parse(Stream input)
    {
        return GetNewInstance().ParseStream(input);
    }

    private IDictionary<string, string?> ParseString(string jsonString)
    {
        _data.Clear();

        var jsonDocumentOptions = new JsonDocumentOptions {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using (JsonDocument doc = JsonDocument.Parse(jsonString, jsonDocumentOptions)) {
            if (doc.RootElement.ValueKind != JsonValueKind.Object) {
                throw new FormatException($"Unsupported JSON token '{doc.RootElement.ValueKind}' was found");
            }

            VisitElement(doc.RootElement);
        }

        return _data;
    }

    private IDictionary<string, string?> ParseStream(Stream input)
    {
        using var reader = new StreamReader(input);
        var jsonString = reader.ReadToEnd();

        return ParseString(jsonString);
    }

    private void VisitElement(JsonElement element)
    {
        foreach (JsonProperty property in element.EnumerateObject()) {
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }
    }

    private void VisitValue(JsonElement value)
    {
        switch (value.ValueKind) {
            case JsonValueKind.Object:
                VisitElement(value);
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement arrayElement in value.EnumerateArray()) {
                    EnterContext(index.ToString());
                    VisitValue(arrayElement);
                    ExitContext();
                    index++;
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string? key = _currentPath;
                if (key != null) {
                    if (_data.ContainsKey(key)) {
                        throw new FormatException($"A duplicate key '{key}' was found.");
                    }

                    _data[key] = value.ToString();
                    break;
                }

                throw new InvalidOperationException($"Null key for JsonElement of kind '{value.ValueKind}', value '{value}'");

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found");
        }
    }

    private void EnterContext(string context)
    {
        _context.Push(context);
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        _context.Pop();
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }
}