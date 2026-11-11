// copied from https://codeberg.org/Barion/Ametrin.Utils/src/branch/main/src/BinaryWriterExtensions.cs

using System.Buffers.Binary;
using System.Diagnostics;

namespace RoBotos.S7.Extensions;

public static class BinaryWriterExtensions
{

    extension(BinaryWriter writer)
    {
        public void WriteBigEndian(float value) => writer.Write(value, sizeof(float), BinaryPrimitives.TryWriteSingleBigEndian);
        public void WriteBigEndian(double value) => writer.Write(value, sizeof(double), BinaryPrimitives.TryWriteDoubleBigEndian);

        public void WriteBigEndian(long value) => writer.Write(value, sizeof(long), BinaryPrimitives.TryWriteInt64BigEndian);
        public void WriteBigEndian(ulong value) => writer.Write(value, sizeof(ulong), BinaryPrimitives.TryWriteUInt64BigEndian);

        public void WriteBigEndian(int value) => writer.Write(value, sizeof(int), BinaryPrimitives.TryWriteInt32BigEndian);
        public void WriteBigEndian(uint value) => writer.Write(value, sizeof(uint), BinaryPrimitives.TryWriteUInt32BigEndian);

        public void WriteBigEndian(short value) => writer.Write(value, sizeof(short), BinaryPrimitives.TryWriteInt16BigEndian);
        public void WriteBigEndian(ushort value) => writer.Write(value, sizeof(ushort), BinaryPrimitives.TryWriteUInt16BigEndian);

        private void Write<T>(T value, int byteSize, Func<Span<byte>, T, bool> converter)
        {
            Span<byte> buffer = stackalloc byte[byteSize];
            var isSuccess = converter(buffer, value);
            Debug.Assert(isSuccess);
            writer.Write(buffer);
        }
    }
}
