namespace RoBotos.S7;

[Obsolete("use ushort directly")]
public readonly record struct Word(ushort Value) : IComparable<Word>
{
    public int CompareTo(Word other) => Value.CompareTo(other.Value);

    public override string ToString() => ToString(null);
    public string ToString(IFormatProvider? formatProvider) => Value.ToString("X", formatProvider);

    public static implicit operator Word(ushort value) => new(value);
    public static implicit operator ushort(Word value) => value.Value;

    public static bool operator <(Word left, Word right) => left.CompareTo(right) < 0;
    public static bool operator <=(Word left, Word right) => left.CompareTo(right) <= 0;
    public static bool operator >(Word left, Word right) => left.CompareTo(right) > 0;
    public static bool operator >=(Word left, Word right) => left.CompareTo(right) >= 0;
}
