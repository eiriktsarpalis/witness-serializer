## Fully type-safe .NET serialization

One important application of source generators today is bringing Native AOT support to libraries that map .NET types to programs. Examples of this include System.Text.Json, ConfigurationBinder, S.C.DataAnnotations, etc. A key constraint of the current crop of generators is that they only work with closed types -- for example having a source generated contract for type `Foo` doesn't imply a source generated contract for `List<Foo>` or other derivative types: `Foo[]`, `Dictionary<string, Foo>`, `List<List<List<Foo>>>`, etc. These need to be configured explicitly.

The [Serde.NET](https://github.com/serdedotnet/serde) and [PolyType](https://github.com/eiriktsarpalis/PolyType) libraries are experiments in building the next generation of .NET serialization. Both lean heavily on static abstracts but the missing ingredient in unlocking full-blown trait driven programming is extension interface implementations.

To give a more concrete example, let's consider a hypothetical interface that lets you serialize .NET values:

```csharp
public interface IJsonSerializable<T>
{    
    static abstract void Write(Utf8JsonWriter writer, T? value);
    static abstract T? Read(ref Utf8JsonReader reader);
}
```

And a serialization API that works with the following signature:

```csharp
public static MyJsonSerializer
{    
    public static string Serialize<T>(T? value) where T : IJsonSerializable<T>
    public static T? Deserialize<T>(string json) where T : IJsonSerializable<T>
}
```

The above is awesome because it enables type-safe serialization but the problem of course is that it only works with types that you immediately control and therefore cannot be extended to things like `int` or `Dictionary<string, SomePoco>`. A common way to work around this issue today is to employ witness types, where you additionally expose the following APIs:

```csharp
public static MyJsonSerializer
{    
    public static string Serialize<T, TWitness>(T? value) where TWitness : IJsonSerializable<T>
    public static T? Deserialize<T, TWitness>(string json) where TWitness : IJsonSerializable<T>
}
```

And then `TWitness` just becomes a class that a source generator can dump implementations of various types into:


```csharp
MyJsonSerializer.Deserialize<int, Witness>("42");

class Witness : IJsonSerializable<int>, IJsonSerializable<string>, IJsonSerializable<SomePoco>, ...
{
    static void IJsonSerializable<int>.Write(Utf8JsonWriter writer, int value) => ...
    static void IJsonSerializable<string>.Write(Utf8JsonWriter writer, string? value) => ...
    static void IJsonSerializable<SomePoco>.Write(Utf8JsonWriter writer, SomePoco? value) => ...
}
```

It's a viable workaround, but obviously the ergonomics of it aren't great from a user perspective. It gets even more finicky if you need to deal with generic types, a canonical implementation for say `List<T>` would need to look as follows:

```csharp
public class ListSerializer<T, TWitness> : IJsonSerializable<List<T>>
    where TWitness : IJsonSerializable<T>
{
    static void IJsonSerializable<List<T>>.Write(Utf8JsonWriter writer, List<T>? value)
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
}
```

Where from the serialization call site you need to craft a witness type like so:

```csharp
JsonSerializer.Serialize<List<int>, ListSerializer<int, IntSerializer>>(value);
```

See the [included project](https://github.com/eiriktsarpalis/witness-serializer/tree/master/src/ConsoleApp) for a working demo of the technique. In practice, this has very obvious UX issues -- witness types need to be crafted manually with multiple nested layers of generics. This is where extension interface implementations could really help out. The witness types shown before could instead be expressed like so:

```csharp
static class SerializerExtensions
{
    extension<T>(List<T> value) : IJsonSerializable<List<T>>
        where T : IJsonSerializable<T> 
    {
        public static void Write(Utf8JsonWriter writer, List<T>? value) { ... }
        public static List<T>? Read(ref Utf8JsonReader reader) { ... }
    }

    extension(int value) : IJsonSerializable<int>
    {
        public static void Write(Utf8JsonWriter writer, int value) { ... }
        public static int Read(ref Utf8JsonReader reader) { ... }
    }
}
```

Having this in scope should let the compiler infer that `List<List<List<int>>>` implements `IJsonSerializable<List<List<List<int>>>>` and therefore calling
```csharp
List<List<List<int>>> value = ...;
MyJsonSerializer.Serialize(value); // type checks
```
Should just work in a fully type-safe and AOT compatible way. This should make writing a serialization libraries substantially simpler -- all an author needs to do is expose the core abstractions and a number of extensions as Horn clauses that the compiler can piece together. This could be further augmented by a source generator that auto-derives interface implementations for user defined POCOs or collection types, in the style of Haskell, Scala, or Rust traits.
