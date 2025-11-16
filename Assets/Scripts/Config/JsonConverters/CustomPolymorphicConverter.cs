using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// Provides a custom JSON converter for polymorphic deserialization of objects of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>This converter supports deserialization of objects based on a "type" property in the JSON payload.
/// Subclasses must implement the <see cref="CreateInstance(string)"/> method to create an instance of the appropriate
/// derived type based on the value of the "type" property.  Writing JSON is not supported by this converter, as
/// indicated by <see cref="CanWrite"/> returning <see langword="false"/>.</remarks>
/// <typeparam name="T">The base type of the objects to be deserialized. The type must have a parameterless constructor.</typeparam>
public abstract class CustomPolymorphicConverter<T> : JsonConverter where T : new()
{
    public override bool CanWrite => false; // Disable writing
    public override bool CanRead => true;
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(T)
            || (objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
                && objectType.GenericTypeArguments[0] == typeof(T));
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        string type = obj["type"]?.ToString();
        T cfg = CreateInstance(type);

        var tempSerializer = new JsonSerializer();
        foreach (var conv in serializer.Converters)
        {
            if (conv is not CustomPolymorphicConverter<T>)
                tempSerializer.Converters.Add(conv);
        }
        tempSerializer.Populate(obj.CreateReader(), cfg);
        return cfg;
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
    protected abstract T CreateInstance(string type);
}