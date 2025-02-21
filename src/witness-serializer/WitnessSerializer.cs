using System.Buffers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WitnessSerializer;

public static class WitnessSerializer
{
    public static string Serialize<T, TWitness>(T? value) where TWitness : IJsonSerializable<T>
    {
        ArrayBufferWriter<byte> bufferWriter = new();
        using var writer = new Utf8JsonWriter(bufferWriter);
        TWitness.Write(writer, value);
        writer.Flush();
        return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
    }

    public static T? Deserialize<T, TWitness>(string json) where TWitness : IJsonSerializable<T>
    {
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return TWitness.Read(ref reader);
    }
}
