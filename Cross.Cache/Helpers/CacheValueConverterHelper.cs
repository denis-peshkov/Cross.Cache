namespace Cross.Cache.Helpers;

public static class CacheValueConverterHelper
{
    public static T? GetConvertedValue<T>(string redisValue)
    {
        if (!string.IsNullOrEmpty(redisValue))
        {
            var type = typeof(T);
            var result = Convert(type, redisValue);
            if (result != null)
            {
                return (T?)result;
            }

            return default;
        }

        return default;
    }
    
    private static object? Convert(Type type, string value)
    {
        if (type == typeof(object))
        {
            return value;
        }

        bool isJsonObject = IsValidJson(value);
        bool isCollection = type.Name != nameof(String) && type.GetInterface(nameof(IEnumerable)) != null;
        
        // Create converter to convert non JSON objects
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        object? result = null;

        try
        {
            // Process collections
            if (isCollection)
            {
                if (isJsonObject)
                {
                    result = JsonSerializer.Deserialize(value, type);
                }
                else
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        result = converter.ConvertFromInvariantString(value);
                    }
                }

                // Special collection handling logic, if the collection is empty - return null
                if (result is ICollection { Count: > 0 } collectionResult)
                {
                    return collectionResult;
                }

                return null;
            }

            // Process regular class objects or System types
            if (isJsonObject)
            {
                result = JsonSerializer.Deserialize(value, type);
            }
            else
            {
                if (converter.CanConvertFrom(typeof(string)))
                {
                    result = converter.ConvertFromInvariantString(value);
                }
            }

            return result;
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
            if (jsonString.StartsWith('{') && jsonString.EndsWith('}'))
            {
                var jsonParsed = JsonValue.Parse(jsonString);
                if (jsonParsed != null)
                {
                    return true;
                }
            }
            else if (jsonString.StartsWith('[') && jsonString.EndsWith(']'))
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