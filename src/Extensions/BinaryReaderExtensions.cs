// copied from https://codeberg.org/Barion/Ametrin.Utils/src/branch/main/src/BinaryReaderExtensions.cs

using System.Buffers.Binary;

namespace RoBotos.S7.Extensions;

public static class BinaryReaderExtensions
{
    extension(BinaryReader reader)
    {
        public double ReadDoubleBigEndian() => reader.Read(BinaryPrimitives.ReadDoubleBigEndian, sizeof(double));
        public float ReadSingleBigEndian() => reader.Read(BinaryPrimitives.ReadSingleBigEndian, sizeof(float));

        public long ReadInt64BigEndian() => reader.Read(BinaryPrimitives.ReadInt64BigEndian, sizeof(long));
        public ulong ReadUInt64BigEndian() => reader.Read(BinaryPrimitives.ReadUInt64BigEndian, sizeof(ulong));

        public int ReadInt32BigEndian() => reader.Read(BinaryPrimitives.ReadInt32BigEndian, sizeof(int));
        public uint ReadUInt32BigEndian() => reader.Read(BinaryPrimitives.ReadUInt32BigEndian, sizeof(uint));

        public short ReadInt16BigEndian() => reader.Read(BinaryPrimitives.ReadInt16BigEndian, sizeof(short));
        public ushort ReadUInt16BigEndian() => reader.Read(BinaryPrimitives.ReadUInt16BigEndian, sizeof(ushort));

        private T Read<T>(Func<ReadOnlySpan<byte>, T> converter, int byteSize) where T : struct
        {
            Span<byte> buffer = stackalloc byte[byteSize];
            reader.ReadExactly(buffer);
            return converter(buffer);
        }
    }
}