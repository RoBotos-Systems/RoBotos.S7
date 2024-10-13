// Class copied from https://github.com/BarionLP/Ametrin.Utils/blob/master/BinaryWriterExtensions.cs

namespace RoBotos.S7;

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
#if RELEASE
        converter(buffer, value);
#endif
#if DEBUG
        if (!converter(buffer, value))
        {
            throw new InvalidOperationException();
        }
#endif
        writer.WriteBigEndian(buffer);
    }

    public static void WriteBigEndian(this BinaryWriter writer, Span<byte> buffer)
    {
        if (BitConverter.IsLittleEndian)
        {
            //do i need to copy?
            buffer.Reverse();
        }
        writer.Write(buffer);
    }

    public delegate bool Converter<T>(Span<byte> buffer, T value);
}
