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
    private Immutable(int number, string text)
    {
        Number = number;
        Text = text;
    }

    public static Immutable Create(Bonus.CodeGen.Optional<int> number = default, Bonus.CodeGen.Optional<string> text = default)
    {
        return new Immutable(number: number.Value, text: text.Value);
    }

    public Immutable With(Bonus.CodeGen.Optional<int> number = default, Bonus.CodeGen.Optional<string> text = default)
    {
        return new Immutable(number: number.ValueOr(Number), text: text.ValueOr(Text));
    }
}
```

## Targets
### must nave
- ~~generate convenience methods for Immutable classes~~
- support for deserialization via Newtonsoft.JSON
- works with dapper

### nice to have
- generate IEquatable&lt;T&gt;
- generate diffs between two immutable classes