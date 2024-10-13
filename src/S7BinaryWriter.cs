﻿using System.Buffers;
using System.Text;

namespace RoBotos.S7;

// this class buffers all written data until Flush or Dispose is called because sps has to get all data in one go
public sealed class S7BinaryWriter(Stream stream, bool _leaveOpen = false) : IDisposable
{
    private readonly BinaryWriter _writer = new(new MemoryStream(), Encoding.ASCII, false);
    private readonly bool _leaveOpen = _leaveOpen;

    private byte _booleanByte = 0;
    private byte _cachedBooleans = 0;
    private const byte BOOLEAN_STOP_BYTE = 0x00;

    public Stream BufferStream => _writer.BaseStream;
    public Stream BaseStream { get; } = stream;


    // has to be called before writing anything else to write the compressed boolean cache to the buffer stream
    private void EndBooleanFlag()
    {
        if (_cachedBooleans == 0)
            return;

        WriteCachedBooleans();
        _writer.Write(BOOLEAN_STOP_BYTE);
    }

    public void WriteWord(Word value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value.Value);
    }

    public void WriteInt(short value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value);
    }

    public void WriteDInt(int value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value);
    }

    public void WriteReal(float value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value);
    }

    public void WriteLReal(double value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value);
    }

    public void WriteString(string value, int maxLength)
    {
        EndBooleanFlag();

        if (value.Length > maxLength)
        {
            throw new ArgumentException("value.Length cannot be larger than maxLength");
        }

        _writer.Write((byte) maxLength);
        _writer.Write((byte) value.Length);

        Span<byte> buffer = stackalloc byte[maxLength];
        Encoding.ASCII.GetBytes(value, buffer);

        _writer.Write(buffer);
    }

    public void WriteDateTime(DateTime dateTime)
    {
        EndBooleanFlag();

        if (dateTime.Year >= 2090 || dateTime.Year < 1990)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), $"Cannot encode {dateTime.Year}. S7 DATE_AND_TIME ranges from 1990 to 2089");
        }

        ReadOnlySpan<byte> buffer = [
            IntToBcd(dateTime.Year % 100),
            IntToBcd(dateTime.Month),
            IntToBcd(dateTime.Day),
            IntToBcd(dateTime.Hour),
            IntToBcd(dateTime.Minute),
            IntToBcd(dateTime.Second),
            IntToBcd(dateTime.Millisecond / 10),
            IntToBcd((dateTime.Millisecond % 10) * 10),
        ];

        _writer.Write(buffer);

        static byte IntToBcd(int value)
        {
            return (byte) (((value / 10) << 4) | (value % 10));
        }
    }

    public void WriteBoolean(bool boolean)
    {
        if (_cachedBooleans >= 8)
        {
            WriteCachedBooleans();
        }

        if (boolean)
        {
            var mask = (byte) (0b1 << _cachedBooleans);
            _booleanByte |= mask;
        }

        _cachedBooleans++;
    }

    private void WriteCachedBooleans()
    {
        _writer.Write(_booleanByte);
        _booleanByte = 0x00;
        _cachedBooleans = 0;
    }

    public void EndStruct()
    {
        EndBooleanFlag();
    }

    public void Flush()
    {
        EndBooleanFlag();
        _writer.Flush();

        var size = (int) BufferStream.Position;
        if (size <= 0)
        {
            return;
        }

        if (size < 256)
        {
            PushImpl(stackalloc byte[size]);
        }
        else
        {
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            PushImpl(buffer.AsSpan()[..size]);
            ArrayPool<byte>.Shared.Return(buffer);
        }

        BaseStream.Flush();

        void PushImpl(Span<byte> buffer)
        {
            BufferStream.Seek(0, SeekOrigin.Begin);
            BufferStream.ReadExactly(buffer);
            BaseStream.Write(buffer);
            BufferStream.Seek(0, SeekOrigin.Begin);
        }
    }

    public void Dispose()
    {
        Flush();

        _writer.Dispose();

        if (_leaveOpen)
        {
            BaseStream.Flush();
        }
        else
        {
            BaseStream.Dispose();
        }
    }
}
