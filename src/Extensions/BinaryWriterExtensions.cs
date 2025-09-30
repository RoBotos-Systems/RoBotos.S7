// Class copied from https://github.com/BarionLP/Ametrin.Utils/blob/main/src/BinaryWriterExtensions.cs

using System.Diagnostics;

namespace RoBotos.S7.Extensions;

public static class BinaryWriterExtensions
{
    public static void WriteBigEndian(this BinaryWriter writer, float value) => writer.WriteBigEndian(value, sizeof(float), BitConverter.TryWriteBytes);
    public static void WriteBigEndian(this BinaryWriter writer, double value) => writer.WriteBigEndian(value, sizeof(double), BitConverter.TryWriteBytes);
    public static void WriteBigEndian(this BinaryWriter writer, short value) => writer.WriteBigEndian(value, sizeof(short), BitConverter.TryWriteBytes);
    public static void WriteBigEndian(this BinaryWriter writer, ushort value) => writer.WriteBigEndian(value, sizeof(ushort), BitConverter.TryWriteBytes);
    public static void WriteBigEndian(this BinaryWriter writer, int value) => writer.WriteBigEndian(value, sizeof(int), BitConverter.TryWriteBytes);
    public static void WriteBigEndian(this BinaryWriter writer, uint value) => writer.WriteBigEndian(value, sizeof(uint), BitConverter.TryWriteBytes);

    public static void WriteBigEndian<T>(this BinaryWriter writer, T value, int byteSize, Converter<T> converter)
    {
        Span<byte> buffer = stackalloc byte[byteSize];
        var success = converter(buffer, value);
        Debug.Assert(success, $"failed to convert {typeof(T).Name} {value} to bytes");
        writer.WriteBigEndian(buffer);
    }

    public static void WriteBigEndian(this BinaryWriter writer, ReadOnlySpan<byte> buffer)
    {
        // copy buffer to allow WriteBigEndian to reverse it
        WriteBigEndian(writer, (Span<byte>)[.. buffer]);
    }

    private static void WriteBigEndian(this BinaryWriter writer, Span<byte> buffer)
    {
        if (BitConverter.IsLittleEndian)
        {
            buffer.Reverse();
        }
        writer.Write(buffer);
    }

    public delegate bool Converter<T>(Span<byte> buffer, T value);
}
