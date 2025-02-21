using System.Text.Json;

namespace WitnessSerializer;

public interface IJsonSerializable<T>
{
    static abstract void Write(Utf8JsonWriter writer, T? value);
    static abstract T? Read(ref Utf8JsonReader reader);
}