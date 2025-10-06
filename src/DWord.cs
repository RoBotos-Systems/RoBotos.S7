namespace RoBotos.S7;

[Obsolete("use uint directly", error: true)]
public readonly record struct DWord(uint Value) : IComparable<DWord>
{
    public int CompareTo(DWord other) => Value.CompareTo(other.Value);

    public override string ToString() => ToString(null);
    public string ToString(IFormatProvider? formatProvider) => Value.ToString("X", formatProvider);

    public static implicit operator DWord(uint value) => new(value);
    public static implicit operator uint(DWord value) => value.Value;

    public static bool operator <(DWord left, DWord right) => left.CompareTo(right) < 0;
    public static bool operator <=(DWord left, DWord right) => left.CompareTo(right) <= 0;
    public static bool operator >(DWord left, DWord right) => left.CompareTo(right) > 0;
    public static bool operator >=(DWord left, DWord right) => left.CompareTo(right) >= 0;
}
