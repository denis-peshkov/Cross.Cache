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
        // Локальная фукнция для проверки является ли JSON объектом строка value
        bool IsValidJson(string jsonString)
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
        
        if (type == typeof(object))
        {
            return value;
        }

        bool isJsonObject = IsValidJson(value);
        bool isCollection = type.Name != nameof(String) && type.GetInterface(nameof(IEnumerable)) != null;
        
        // Создание конвертера для конвертации не JSON объектов
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        object? result = null;

        try
        {
            // Особая логика проверки если вычитываем коллекцию, если коллекция пустая - вернем null
            if (isCollection)
            {
                // Конверсия для других типов коллекций
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

                if (result is ICollection { Count: > 0 } collectionResult)
                {
                    return collectionResult;
                }

                return null;
            }

            // Конверсия в одиночные объекты или системные типы данных
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
}