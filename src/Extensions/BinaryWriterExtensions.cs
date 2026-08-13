// copied from https://github.com/BarionLP/Ametrin.Utils/blob/main/src/BinaryWriterExtensions.cs

using System.Buffers.Binary;
using System.Diagnostics;

namespace RoBotos.S7.Extensions;

public static class BinaryWriterExtensions
{

    public static void WriteBigEndian(this BinaryWriter writer, float value) => writer.Write(value, sizeof(float), BinaryPrimitives.TryWriteSingleBigEndian);
    public static void WriteBigEndian(this BinaryWriter writer, double value) => writer.Write(value, sizeof(double), BinaryPrimitives.TryWriteDoubleBigEndian);

    public static void WriteBigEndian(this BinaryWriter writer, long value) => writer.Write(value, sizeof(long), BinaryPrimitives.TryWriteInt64BigEndian);
    public static void WriteBigEndian(this BinaryWriter writer, ulong value) => writer.Write(value, sizeof(ulong), BinaryPrimitives.TryWriteUInt64BigEndian);

    public static void WriteBigEndian(this BinaryWriter writer, int value) => writer.Write(value, sizeof(int), BinaryPrimitives.TryWriteInt32BigEndian);
    public static void WriteBigEndian(this BinaryWriter writer, uint value) => writer.Write(value, sizeof(uint), BinaryPrimitives.TryWriteUInt32BigEndian);

    public static void WriteBigEndian(this BinaryWriter writer, short value) => writer.Write(value, sizeof(short), BinaryPrimitives.TryWriteInt16BigEndian);
    public static void WriteBigEndian(this BinaryWriter writer, ushort value) => writer.Write(value, sizeof(ushort), BinaryPrimitives.TryWriteUInt16BigEndian);

    private static void Write<T>(this BinaryWriter writer, T value, int byteSize, Converter<T> converter)
    {
        Span<byte> buffer = stackalloc byte[byteSize];
        var isSuccess = converter(buffer, value);
        Debug.Assert(isSuccess);
        writer.Write(buffer);
    }

    public delegate bool Converter<T>(Span<byte> buffer, T value);
}
