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

## Targets
### must nave
- [X] generate convenience methods for Immutable classes
- [X] support for deserialization via Newtonsoft.JSON
- [X] works with dapper

### nice to have
- [ ] replace ObsoleteAttribute on generated constructor with DiagnosticAnalyzer
- [ ] generate IEquatable&lt;T&gt;
- [ ] generate diffs between two immutable classes