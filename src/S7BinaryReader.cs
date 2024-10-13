﻿using System.Text;

namespace RoBotos.S7;

public sealed class S7BinaryReader(Stream stream, bool _leaveOpen = false) : IDisposable
{
    private readonly BinaryReader _reader = new(stream, Encoding.ASCII, _leaveOpen);
    private byte _booleanByte = 0;
    private byte _extractedBools = 0;
    private bool _wasLastBoolean = false;
    public Stream BaseStream => _reader.BaseStream;

    // has to be called before reading anything
    private void EndBooleanFlag()
    {
        if (!_wasLastBoolean)
            return;

        _extractedBools = 0;
        _wasLastBoolean = false;
        _reader.ReadByte(); //skip boolean stop byte
    }

    public Word ReadWord()
    {
        EndBooleanFlag();
        return new(_reader.ReadUInt16BigEndian());
    }

    public short ReadInt()
    { // awl int is 16 bits
        EndBooleanFlag();
        return _reader.ReadInt16BigEndian();
    }

    public int ReadDInt()
    {
        EndBooleanFlag();
        return _reader.ReadInt32BigEndian();
    }

    public float ReadReal()
    {
        EndBooleanFlag();
        return _reader.ReadSingleBigEndian();
    }

    public double ReadLReal()
    {
        EndBooleanFlag();
        return _reader.ReadDoubleBigEndian();
    }


    public string ReadString(int expectedSize)
    {
        EndBooleanFlag();
        var maxSize = _reader.ReadByte();
        var actualSize = _reader.ReadByte();

        if (maxSize != expectedSize)
        {
            throw new InvalidDataException($"Expected String [{expectedSize}], got String [{maxSize}] ");
        }

        Span<byte> buffer = stackalloc byte[maxSize];

        var bytesRead = _reader.Read(buffer);

        if (bytesRead != maxSize)
        {
            throw new InvalidDataException($"Expected {maxSize} bytes got {bytesRead}");
        }

        return Encoding.ASCII.GetString(buffer[..actualSize]);
    }

    public DateTime ReadDateTime()
    {
        EndBooleanFlag();
        Span<byte> buffer = stackalloc byte[8];
        var bytesRead = _reader.Read(buffer);

        if (bytesRead != 8)
        {
            throw new InvalidDataException($"Invalid DATE_AND_TIME format. Expected 8 bytes. Got {bytesRead} before end of stream");
        }

        var year = BcdToInt(buffer[0]);
        var month = BcdToInt(buffer[1]);
        var day = BcdToInt(buffer[2]);
        var hour = BcdToInt(buffer[3]);
        var minute = BcdToInt(buffer[4]);
        var second = BcdToInt(buffer[5]);
        var millisecond = BcdToInt(buffer[6]) * 10 + BcdToInt(buffer[7]) / 100;

        var century = (year < 90) ? 2000 : 1900;
        var dateTime = new DateTime(century + year, month, day, hour, minute, second, millisecond);

        return dateTime;

        static int BcdToInt(byte bcd)
        {
            return ((bcd >> 4) * 10) + (bcd & 0x0F);
        }
    }

    public bool ReadBoolean()
    {
        if (_extractedBools >= 8 || !_wasLastBoolean)
        {
            _booleanByte = _reader.ReadByte();
            _extractedBools = 0;
        }

        var boolean = (_booleanByte & 1) == 1;
        _booleanByte >>= 1;
        _extractedBools++;

        _wasLastBoolean = true;

        return boolean;
    }

    public void EndStruct()
    {
        EndBooleanFlag();
    }


    public void Dispose()
    {
        _reader.Dispose();
    }
}
