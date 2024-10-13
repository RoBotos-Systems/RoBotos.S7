// Class copied from https://github.com/BarionLP/Ametrin.Utils/blob/master/BinaryReaderExtensions.cs

namespace RoBotos.S7;

public static class BinaryReaderExtensions
{
    public static double ReadDoubleBigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToDouble, 8);
    public static float ReadSingleBigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToSingle, 4);

    public static int ReadInt32BigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToInt32, 4);
    public static uint ReadUInt32BigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToUInt32, 4);

    public static short ReadInt16BigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToInt16, 2);
    public static ushort ReadUInt16BigEndian(this BinaryReader reader) => reader.ReadBigEndian(BitConverter.ToUInt16, 2);

    public static T ReadBigEndian<T>(this BinaryReader reader, Converter<T> converter, int byteSize) where T : struct
    {
        Span<byte> buffer = stackalloc byte[byteSize];
        reader.ReadBigEndian(buffer);
        return converter(buffer);
    }

    public static void ReadBigEndian(this BinaryReader reader, Span<byte> buffer)
    {
        reader.Read(buffer);
        if (BitConverter.IsLittleEndian)
        {
            buffer.Reverse();
        }
    }

    public delegate T Converter<out T>(ReadOnlySpan<byte> buffer);
}
