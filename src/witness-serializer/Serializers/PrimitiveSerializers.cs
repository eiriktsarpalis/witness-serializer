using System.Text.Json;

namespace WitnessSerializer;

public sealed class IntSerializer : IJsonSerializable<int>
{
    public static void Write(Utf8JsonWriter writer, int value) => writer.WriteNumberValue(value);
    public static int Read(ref Utf8JsonReader reader) => reader.GetInt32();
}

public sealed class StringSerializer : IJsonSerializable<string>
{
    public static void Write(Utf8JsonWriter writer, string? value) => writer.WriteStringValue(value);
    public static string? Read(ref Utf8JsonReader reader) => reader.GetString();
}

public sealed class BoolSerializer : IJsonSerializable<bool>
{
    public static void Write(Utf8JsonWriter writer, bool value) => writer.WriteBooleanValue(value);
    public static bool Read(ref Utf8JsonReader reader) => reader.GetBoolean();
}

public sealed class DoubleSerializer : IJsonSerializable<double>
{
    public static void Write(Utf8JsonWriter writer, double value) => writer.WriteNumberValue(value);
    public static double Read(ref Utf8JsonReader reader) => reader.GetDouble();
}