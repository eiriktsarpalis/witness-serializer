using System.Text.Json;

namespace WitnessSerializer;

public sealed class ArraySerializer<T, TWitness> : IJsonSerializable<T[]>
    where TWitness : IJsonSerializable<T>
{
    public static void Write(Utf8JsonWriter writer, T[]? value)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (T item in value)
        {
            TWitness.Write(writer, item);
        }
        writer.WriteEndArray();
    }

    public static T[]? Read(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                reader.Read();
                return null;
            case not JsonTokenType.StartArray:
                throw new JsonException();
        }

        reader.Read();
        List<T> list = [];
        while (reader.TokenType is not JsonTokenType.EndArray)
        {
            list.Add(TWitness.Read(ref reader)!);
            reader.Read();
        }
        reader.Read();
        return [.. list];
    }
}
