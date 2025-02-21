using WitnessSerializer;

// Construct a witness type for the value by manually folding together generic witness types.
// Assuming the language had support for extension interface implementations, these witness
// types could directly replaced with extensions, and the appropriate interface implementation
// would be resolved by the compiler. This is similar to how trait implementations are being
// resolved in languages like Haskell, Rust, and Scala.
using Witness = 
    WitnessSerializer.DictionarySerializer<
        (Person, double[]), 
        WitnessSerializer.TupleSerializer<
            Person, Person,
            double[], WitnessSerializer.ArraySerializer<double, WitnessSerializer.DoubleSerializer>>>;

Dictionary<string, (Person, double[])> value = new() { ["key"] = (new Person("John", 39), [3.14, -1.1]) };

string json = WitnessSerializer.WitnessSerializer.Serialize<Dictionary<string, (Person, double[])>, Witness>(value);
Console.WriteLine(json);

value = WitnessSerializer.WitnessSerializer.Deserialize<Dictionary<string, (Person, double[])>, Witness>(json)!;
Console.WriteLine(value["key"]);

partial record class Person(string Name, int Age);