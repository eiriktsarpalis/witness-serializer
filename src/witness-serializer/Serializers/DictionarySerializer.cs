using System.Text.Json;

namespace WitnessSerializer;

public sealed class DictionarySerializer<TValue, TWitness> : IJsonSerializable<Dictionary<string, TValue>>
    where TWitness : IJsonSerializable<TValue>
{
    public static void Write(Utf8JsonWriter writer, Dictionary<string, TValue>? value)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (KeyValuePair<string, TValue> item in value)
        {
            writer.WritePropertyName(item.Key);
            TWitness.Write(writer, item.Value);
        }
        writer.WriteEndObject();
    }

    public static Dictionary<string, TValue>? Read(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                reader.Read();
                return null;
            case not JsonTokenType.StartObject:
                throw new JsonException();
        }

        Dictionary<string, TValue> dictionary = [];
        reader.Read();
        while (reader.TokenType is not JsonTokenType.EndObject)
        {
            string key = reader.GetString()!;
            reader.Read();
            TValue value = TWitness.Read(ref reader)!;
            dictionary.Add(key, value);
            reader.Read();
        }
        reader.Read();
        return dictionary;
    }
}
