namespace Bonus.CodeGen
{
    public struct Optional<T>
    {
        public Optional(T value)
        {
            IsSet = true;
            Value = value;
        }

        public bool IsSet { get; }
        public T Value { get; }

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public T ValueOr(T value)
        {
            return IsSet ? Value : value;
        }
    }
}