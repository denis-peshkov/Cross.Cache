namespace Cross.Cache.Helpers;

public static class CacheValueConverterHelper
{
    public static T? GetConvertedValue<T>(string? redisValue)
    {
        if (string.IsNullOrEmpty(redisValue))
            return default;

        var type = typeof(T);
        var result = Convert(type, redisValue);

        return result != null
            ? (T?)result
            : default;
    }

    private static object? Convert(Type type, string value)
    {
        if (type == typeof(object))
            return value;

        try
        {
            var isJson = IsValidJson(value);
            var isCollection = type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
            // Create converter to convert non JSON objects
            var converter = TypeDescriptor.GetConverter(type);

            // Process regular class objects or System types
            if (isJson)
            {
                var result = JsonSerializer.Deserialize(value, type);

                // Special collection handling logic, if the collection is empty - return null
                if (isCollection && result is ICollection { Count: 0 })
                    return null;

                return result;
            }

            if (converter.CanConvertFrom(typeof(string)))
            {
                var result = converter.ConvertFromInvariantString(value);

                // Special collection handling logic, if the collection is empty - return null
                if (isCollection && result is ICollection { Count: 0 })
                    return null;

                return result;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            if ((jsonString.StartsWith('{') && jsonString.EndsWith('}'))
                || (jsonString.StartsWith('[') && jsonString.EndsWith(']')))
            {
                var jsonParsed = JsonValue.Parse(jsonString);
                if (jsonParsed != null)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
