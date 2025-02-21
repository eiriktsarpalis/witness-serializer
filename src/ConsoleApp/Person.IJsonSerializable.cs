using System.Text.Json;
using WitnessSerializer;

// Possible source generated implementation for IJsonSerializable<Person>
partial record class Person : IJsonSerializable<Person>
{
    static void IJsonSerializable<Person>.Write(Utf8JsonWriter writer, Person? value)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(Name));
        StringSerializer.Write(writer, value.Name);
        writer.WritePropertyName(nameof(Age));
        IntSerializer.Write(writer, value.Age);
        writer.WriteEndObject();
    }

    static Person? IJsonSerializable<Person>.Read(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                reader.Read();
                return null;
            case not JsonTokenType.StartObject:
                throw new JsonException();
        }

        string name = default!;
        int age = default!;
        reader.Read();
        while (reader.TokenType is not JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString()!;
            reader.Read();
            switch (propertyName)
            {
                case "Name":
                    name = StringSerializer.Read(ref reader)!;
                    break;
                case "Age":
                    age = IntSerializer.Read(ref reader);
                    break;
                default:
                    throw new JsonException();
            }
            reader.Read();
        }

        return new Person(name, age);
    }
}