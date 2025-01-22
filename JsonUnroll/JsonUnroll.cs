using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace JsonUnroll;

public static class JsonUnroll
{
    public static JToken Prepare(string json) => JToken.Parse(json);

    public static List<Dictionary<string, object>> Flatten(
        JToken token,
        Dictionary<string, object> parent = null,
        string prefix = "")
    {
        return token switch
        {
            JObject jsonObject => FlattenJsonObject(jsonObject, parent, prefix),
            JArray jsonArray => FlattenJsonArray(jsonArray, parent, prefix),
            _ => FlattenPrimitive(token, parent, prefix)
        };
    }

    private static List<Dictionary<string, object>> FlattenJsonObject(
        JObject jsonObject,
        Dictionary<string, object> parent,
        string prefix)
    {
        var currentRows = InitializeCurrentRows(parent);

        foreach (var property in jsonObject.Properties())
        {
            var key = GetCombinedKey(prefix, property.Name);
            currentRows = ProcessToken(currentRows, key, property.Value);
        }

        return currentRows;
    }

    private static List<Dictionary<string, object>> FlattenJsonArray(
        JArray jsonArray,
        Dictionary<string, object> parent,
        string prefix)
    {
        var result = new List<Dictionary<string, object>>();

        foreach (var item in jsonArray)
        {
            // Correctly handle null parent by initializing a new dictionary
            var itemParent = parent != null
                ? new Dictionary<string, object>(parent)
                : new Dictionary<string, object>();

            var itemRows = Flatten(item, itemParent, prefix);
            result.AddRange(itemRows);
        }

        return result;
    }

    private static List<Dictionary<string, object>> FlattenPrimitive(
        JToken token,
        Dictionary<string, object> parent,
        string prefix)
    {
        var row = new Dictionary<string, object>(parent ?? new Dictionary<string, object>());

        if (!string.IsNullOrEmpty(prefix))
        {
            row[prefix] = token.ToObject<object>();
        }

        return new List<Dictionary<string, object>> { row };
    }
    private static List<Dictionary<string, object>> ProcessToken(
        List<Dictionary<string, object>> currentRows,
        string key,
        JToken value)
    {
        return value switch
        {
            JArray array => ProcessArray(currentRows, key, array),
            JObject nestedObject => ProcessObject(currentRows, key, nestedObject),
            _ => ProcessPrimitive(currentRows, key, value)
        };
    }

    private static List<Dictionary<string, object>> ProcessArray(
        List<Dictionary<string, object>> currentRows,
        string key,
        JArray array)
    {
        var newRows = new List<Dictionary<string, object>>();

        foreach (var row in currentRows)
        {
            if (array.Count == 0)
            {
                newRows.Add(new Dictionary<string, object>(row));
            }
            else
            {
                foreach (var item in array)
                {
                    var newRow = new Dictionary<string, object>(row);

                    if (item is JObject itemObject)
                    {
                        var nestedRows = Flatten(itemObject, new Dictionary<string, object>(), key);
                        if (nestedRows.Count > 0)
                        {
                            foreach (var nestedRow in nestedRows)
                            {
                                var mergedRow = MergeRows(newRow, nestedRow);
                                newRows.Add(mergedRow);
                            }
                        }
                        else
                        {
                            newRows.Add(newRow);
                        }
                    }
                    else
                    {
                        newRow[key] = item.ToObject<object>();
                        newRows.Add(newRow);
                    }
                }
            }
        }

        return newRows;
    }

    private static List<Dictionary<string, object>> ProcessObject(
        List<Dictionary<string, object>> currentRows,
        string key,
        JObject nestedObject)
    {
        var nestedRows = Flatten(nestedObject, new Dictionary<string, object>(), key);
        var mergedRows = new List<Dictionary<string, object>>();

        foreach (var row in currentRows)
        {
            foreach (var nestedRow in nestedRows)
            {
                var mergedRow = MergeRows(row, nestedRow);
                mergedRows.Add(mergedRow);
            }
        }

        return mergedRows;
    }

    private static List<Dictionary<string, object>> ProcessPrimitive(
        List<Dictionary<string, object>> currentRows,
        string key,
        JToken value)
    {
        var newRows = new List<Dictionary<string, object>>();

        foreach (var row in currentRows)
        {
            var newRow = new Dictionary<string, object>(row);
            newRow[key] = value.ToObject<object>();
            newRows.Add(newRow);
        }

        return newRows;
    }

    private static List<Dictionary<string, object>> InitializeCurrentRows(Dictionary<string, object> parent)
    {
        var initialRow = parent != null
            ? new Dictionary<string, object>(parent)
            : new Dictionary<string, object>();

        return new List<Dictionary<string, object>> { initialRow };
    }

    private static Dictionary<string, object> MergeRows(
        Dictionary<string, object> row,
        Dictionary<string, object> nestedRow)
    {
        var merged = new Dictionary<string, object>(row);
        foreach (var kvp in nestedRow)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }

    private static string GetCombinedKey(string prefix, string name)
    {
        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
    }
}

public static class JsonUnroll2
{
    public static JsonNode Prepare(string json) => JsonNode.Parse(json);

    public static List<Dictionary<string, object>> Flatten(
        JsonNode token,
        Dictionary<string, object> parent = null,
        string prefix = "")
    {
        if (token == null)
        {
            return FlattenPrimitive(null, parent, prefix);
        }

        return token switch
        {
            JsonObject jsonObject => FlattenJsonObject(jsonObject, parent, prefix),
            JsonArray jsonArray => FlattenJsonArray(jsonArray, parent, prefix),
            _ => FlattenPrimitive(token, parent, prefix)
        };
    }

    private static List<Dictionary<string, object>> FlattenJsonObject(
        JsonObject jsonObject,
        Dictionary<string, object> parent,
        string prefix)
    {
        var currentRows = InitializeCurrentRows(parent);

        foreach (var property in jsonObject)
        {
            var key = GetCombinedKey(prefix, property.Key);
            currentRows = ProcessToken(currentRows, key, property.Value);
        }

        return currentRows;
    }

    private static List<Dictionary<string, object>> FlattenJsonArray(
        JsonArray jsonArray,
        Dictionary<string, object> parent,
        string prefix)
    {
        var result = new List<Dictionary<string, object>>();

        foreach (var item in jsonArray)
        {
            var itemParent = parent != null
                ? new Dictionary<string, object>(parent)
                : new Dictionary<string, object>();

            var itemRows = Flatten(item, itemParent, prefix);
            result.AddRange(itemRows);
        }

        return result;
    }

    private static List<Dictionary<string, object>> FlattenPrimitive(
        JsonNode token,
        Dictionary<string, object> parent,
        string prefix)
    {
        var row = new Dictionary<string, object>(parent ?? new Dictionary<string, object>());

        if (!string.IsNullOrEmpty(prefix))
        {
            object value = null;
            if (token is JsonValue jsonValue)
            {
                value = jsonValue.GetValue<object>();
            }
            row[prefix] = value;
        }

        return new List<Dictionary<string, object>> { row };
    }

    private static List<Dictionary<string, object>> ProcessToken(
        List<Dictionary<string, object>> currentRows,
        string key,
        JsonNode value)
    {
        if (value == null)
        {
            return ProcessPrimitive(currentRows, key, null);
        }

        return value switch
        {
            JsonArray array => ProcessArray(currentRows, key, array),
            JsonObject nestedObject => ProcessObject(currentRows, key, nestedObject),
            _ => ProcessPrimitive(currentRows, key, value)
        };
    }

    private static List<Dictionary<string, object>> ProcessArray(
        List<Dictionary<string, object>> currentRows,
        string key,
        JsonArray array)
    {
        var newRows = new List<Dictionary<string, object>>();

        foreach (var row in currentRows)
        {
            if (array.Count == 0)
            {
                newRows.Add(new Dictionary<string, object>(row));
            }
            else
            {
                foreach (var item in array)
                {
                    var newRow = new Dictionary<string, object>(row);

                    if (item is JsonObject itemObject)
                    {
                        var nestedRows = Flatten(itemObject, new Dictionary<string, object>(), key);
                        if (nestedRows.Count > 0)
                        {
                            foreach (var nestedRow in nestedRows)
                            {
                                var mergedRow = MergeRows(newRow, nestedRow);
                                newRows.Add(mergedRow);
                            }
                        }
                        else
                        {
                            newRows.Add(newRow);
                        }
                    }
                    else
                    {
                        newRow[key] = item == null ? null : ConvertNodeToObject(item);
                        newRows.Add(newRow);
                    }
                }
            }
        }

        return newRows;
    }

    private static List<Dictionary<string, object>> ProcessObject(
        List<Dictionary<string, object>> currentRows,
        string key,
        JsonObject nestedObject)
    {
        var nestedRows = Flatten(nestedObject, new Dictionary<string, object>(), key);
        var mergedRows = new List<Dictionary<string, object>>();

        foreach (var row in currentRows)
        {
            foreach (var nestedRow in nestedRows)
            {
                var mergedRow = MergeRows(row, nestedRow);
                mergedRows.Add(mergedRow);
            }
        }

        return mergedRows;
    }

    private static List<Dictionary<string, object>> ProcessPrimitive(
        List<Dictionary<string, object>> currentRows,
        string key,
        JsonNode value)
    {
        var newRows = new List<Dictionary<string, object>>();

        object primitiveValue = null;
        if (value is JsonValue jsonValue)
        {
            primitiveValue = jsonValue.GetValue<object>();
        }
        else if (value != null)
        {
            throw new NotSupportedException("Unexpected node type in ProcessPrimitive");
        }

        foreach (var row in currentRows)
        {
            var newRow = new Dictionary<string, object>(row);
            newRow[key] = primitiveValue;
            newRows.Add(newRow);
        }

        return newRows;
    }

    private static object ConvertNodeToObject(JsonNode node)
    {
        if (node == null)
        {
            return null;
        }

        return node switch
        {
            JsonObject obj => obj.ToDictionary(prop => prop.Key, prop => ConvertNodeToObject(prop.Value)),
            JsonArray arr => arr.Select(item => ConvertNodeToObject(item)).ToList(),
            JsonValue val => val.GetValue<object>(),
            _ => throw new NotSupportedException($"Unsupported node type: {node.GetType()}")
        };
    }

    private static List<Dictionary<string, object>> InitializeCurrentRows(Dictionary<string, object> parent)
    {
        var initialRow = parent != null
            ? new Dictionary<string, object>(parent)
            : new Dictionary<string, object>();

        return new List<Dictionary<string, object>> { initialRow };
    }

    private static Dictionary<string, object> MergeRows(
        Dictionary<string, object> row,
        Dictionary<string, object> nestedRow)
    {
        var merged = new Dictionary<string, object>(row);
        foreach (var kvp in nestedRow)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }

    private static string GetCombinedKey(string prefix, string name)
    {
        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
    }
}
