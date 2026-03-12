// Class copied from https://github.com/BarionLP/Ametrin.Utils/blob/main/src/BinaryReaderExtensions.cs

using System.Buffers.Binary;

namespace RoBotos.S7.Extensions;

public static class BinaryReaderExtensions
{
    public static double ReadDoubleBigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadDoubleBigEndian, sizeof(double));
    public static float ReadSingleBigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadSingleBigEndian, sizeof(float));

    public static long ReadInt64BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadInt64BigEndian, sizeof(long));
    public static ulong ReadUInt64BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadUInt64BigEndian, sizeof(ulong));
    
    public static int ReadInt32BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadInt32BigEndian, sizeof(int));
    public static uint ReadUInt32BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadUInt32BigEndian, sizeof(uint));

    public static short ReadInt16BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadInt16BigEndian, sizeof(short));
    public static ushort ReadUInt16BigEndian(this BinaryReader reader) => reader.Read(BinaryPrimitives.ReadUInt16BigEndian, sizeof(ushort));

    private static T Read<T>(this BinaryReader reader, Converter<T> converter, int byteSize) where T : struct
    {
        Span<byte> buffer = stackalloc byte[byteSize];
        reader.ReadExactly(buffer);
        return converter(buffer);
    }

    private delegate T Converter<out T>(ReadOnlySpan<byte> buffer);
}
