namespace RoBotos.S7;

public readonly record struct LWord(ulong Value) : IComparable<LWord>
{
    public int CompareTo(LWord other) => Value.CompareTo(other.Value);

    public override string ToString() => ToString(null);
    public string ToString(IFormatProvider? formatProvider) => Value.ToString("X", formatProvider);

    public static implicit operator LWord(ulong value) => new(value);
    public static implicit operator ulong(LWord value) => value.Value;

    public static bool operator <(LWord left, LWord right) => left.CompareTo(right) < 0;
    public static bool operator <=(LWord left, LWord right) => left.CompareTo(right) <= 0;
    public static bool operator >(LWord left, LWord right) => left.CompareTo(right) > 0;
    public static bool operator >=(LWord left, LWord right) => left.CompareTo(right) >= 0;
}