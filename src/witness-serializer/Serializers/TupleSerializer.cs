using System.Text.Json;

namespace WitnessSerializer;

public sealed class TupleSerializer<T1, TWitness1, T2, TWitness2> : IJsonSerializable<(T1, T2)>
    where TWitness1 : IJsonSerializable<T1>
    where TWitness2 : IJsonSerializable<T2>
{
    public static void Write(Utf8JsonWriter writer, (T1, T2) value)
    {
        writer.WriteStartArray();
        TWitness1.Write(writer, value.Item1);
        TWitness2.Write(writer, value.Item2);
        writer.WriteEndArray();
    }

    public static (T1, T2) Read(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                reader.Read();
                return default;

            case not JsonTokenType.StartArray:
                throw new JsonException();
        }

        reader.Read();
        T1 item1 = TWitness1.Read(ref reader)!;
        reader.Read();
        T2 item2 = TWitness2.Read(ref reader)!;

        if (reader.TokenType is not JsonTokenType.EndArray)
        {
            throw new JsonException();
        }

        return (item1, item2);
    }
}
