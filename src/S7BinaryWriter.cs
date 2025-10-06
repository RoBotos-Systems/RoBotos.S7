using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using RoBotos.S7.Extensions;

namespace RoBotos.S7;

/// <summary>
/// A specialized stream writer following the S7 encoding for primitive data types.<br/>
/// The method names follow the S7 data type names and not the C# names!<br/>
/// Flush has to be called for the data to be written into the stream.<br/>
/// This is because Siemens PLCs expect the entire data package at once
/// </summary>
/// <param name="stream">The stream to write into</param>
/// <param name="leaveOpen">Whether the stream should be left open when Disposed is called</param>
public sealed class S7BinaryWriter(Stream stream, bool leaveOpen = false) : IDisposable
{
    private readonly BinaryWriter _writer = new(new MemoryStream(), Encoding.ASCII, false);
    private readonly bool _leaveOpen = leaveOpen;

    private byte _booleanByte = 0;
    private byte _cachedBooleans = 0;
    private const byte BOOLEAN_STOP_BYTE = 0x00;

    private bool _isDisposed = false;

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

    [Experimental("S7001")]
    public void WriteByte(byte value)
    {
        // don't think this works this way
        EndBooleanFlag();
        _writer.Write(value);
    }
    public void WriteWord(ushort value)
    {
        EndBooleanFlag();
        _writer.WriteBigEndian(value);
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

    public void WriteUDInt(uint value)
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

        if (!IsAscii(value))
        {
            throw new ArgumentException("string can only contain ascii characters", nameof(value));
        }

        if (value.Length > maxLength)
        {
            throw new ArgumentException("value.Length cannot be larger than maxLength");
        }

        _writer.Write((byte)maxLength);
        _writer.Write((byte)value.Length);

        Span<byte> buffer = stackalloc byte[maxLength];
        var bytesWritten = Encoding.ASCII.GetBytes(value, buffer);
        Debug.Assert(bytesWritten == value.Length);

        _writer.Write(buffer);

        static bool IsAscii(string s)
            => s.AsSpan().IndexOfAnyExceptInRange('\u0000', '\u007F') < 0;
    }

    public void WriteTime(TimeSpan time)
    {
        if (time.TotalMilliseconds > int.MaxValue || time.TotalMilliseconds < int.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(time), $"{time} is too large/small for S7 32-bit TIME");
        }
        WriteDInt((int)time.TotalMilliseconds);
    }

    public static DateTime MinDateTime { get; } = new(1990, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Unspecified);
    public static DateTime MaxDateTime { get; } = new(2089, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified);
    public void WriteDateTime(DateTime dateTime)
    {
        EndBooleanFlag();

        if (dateTime > MaxDateTime || dateTime < MinDateTime)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), $"Cannot encode {dateTime}. S7 DATE_AND_TIME ranges from {MinDateTime} to {MaxDateTime}");
        }

        Span<byte> buffer = [
            IntToBcd(dateTime.Year % 100),
            IntToBcd(dateTime.Month),
            IntToBcd(dateTime.Day),
            IntToBcd(dateTime.Hour),
            IntToBcd(dateTime.Minute),
            IntToBcd(dateTime.Second),
            IntToBcd(dateTime.Millisecond / 10),
            IntToBcd((dateTime.Millisecond % 10) * 10),
        ];

        buffer[^1] |= (byte)(dateTime.DayOfWeek + 1);

        _writer.Write(buffer);

        static byte IntToBcd(int value)
        {
            return (byte)(((value / 10) << 4) | (value % 10));
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
            var mask = (byte)(0b1 << _cachedBooleans);
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

        var size = (int)BufferStream.Position;
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

        if (_isDisposed)
        {
            return;
        }

        Flush();

        _writer.Dispose();

        if (!_leaveOpen)
        {
            BaseStream.Dispose();
        }

        _isDisposed = true;
    }
}
