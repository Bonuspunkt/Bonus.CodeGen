# Bonus.CodeGen
[![Build status](https://ci.appveyor.com/api/projects/status/lex1rpcfcqt9fpad?svg=true)](https://ci.appveyor.com/project/Bonuspunkt/bonus-codegen)

## Immutable Generation

``` csharp
// mark class with GenerateImmutable Attribute
[GenerateImmutable]
public partial class Immutable {
    public int Number { get; }
    public string Text { get; }
}

// will generate at compile time
partial class Immutable
{
    public Immutable(int number, string text)
    {
        Number = number;
        Text = text;
    }

    public static Immutable Create(Optional<int> number = default, Optional<string> text = default)
    {
        return new Immutable(number: number.Value, text: text.Value);
    }

    public Immutable With(Optional<int> number = default, Optional<string> text = default)
    {
        return new Immutable(number: number.ValueOr(Number), text: text.ValueOr(Text));
    }
}


// DefaultValues
public partial class Immutable {
    public static readonly Immutable New = new Immutable(42, "answer");
}

var immutable = Immutable.Create();
// immutable.Number == 42;
// immutable.Text == "answer";
```

## IEquatable&lt;T&gt; Generation
``` csharp
// mark class with GenerateEquatable Attribute
[GenerateEquatable]
partial class Equatable
{
    public int Number { get; set; }
    public string Text { get; set; }
    public IEnumerable<int> Numbers { get; set; }
}

// will generate at compile time
partial class Equatable : System.IEquatable<Equatable>
{
    public override bool Equals(object obj)
    {
        return Equals(obj as Equatable);
    }

    public bool Equals(Equatable other)
    {
        return other != null && 
            Equals(Number, other.Number) && 
            Equals(Text, other.Text) && 
            System.Linq.Enumerable.SequenceEqual(Numbers, other.Numbers);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            const int HashingBase = (int)2166136261U;
            const int HashingMultiplier = 16777619;
            var hashCode = HashingBase;
            hashCode = (hashCode * HashingMultiplier) ^ Number.GetHashCode();
            hashCode = (hashCode * HashingMultiplier) ^ (Text?.GetHashCode() ?? 0);
            hashCode = (hashCode * HashingMultiplier) ^ (Numbers?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

```


## Targets
### must nave
- [X] generate convenience methods for Immutable classes
- [X] support for deserialization via [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [X] works with [Dapper](https://github.com/StackExchange/Dapper/)

### nice to have
- [X] generate IEquatable&lt;T&gt;
- [ ] replace ObsoleteAttribute on generated constructor with DiagnosticAnalyzer
- [ ] provide CodeFixProvider for Analyzer warnings
- [ ] generate diffs between two immutable objects