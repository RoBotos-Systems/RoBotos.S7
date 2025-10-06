namespace RoBotos.S7.Test;

public sealed class Tests
{
    [Test]
    public async Task ReadWriteDateTime()
    {
        using var stream = new MemoryStream();
        using var writer = new S7BinaryWriter(stream);
        using var reader = new S7BinaryReader(stream);

        var now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond / 10 * 10);

        writer.WriteDateTime(S7BinaryWriter.MinDateTime);
        writer.WriteDateTime(now);
        writer.WriteDateTime(S7BinaryWriter.MaxDateTime);

        writer.Flush();
        stream.Position = 0;

        await Assert.That(reader.ReadDateTime(DateTimeKind.Utc)).IsEqualTo(S7BinaryWriter.MinDateTime);
        await Assert.That(reader.ReadDateTime(DateTimeKind.Local)).IsEqualTo(now);
        await Assert.That(reader.ReadDateTime(DateTimeKind.Utc)).IsEqualTo(S7BinaryWriter.MaxDateTime);

        await Assert.That(() => writer.WriteDateTime(S7BinaryWriter.MinDateTime.AddTicks(-1))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => writer.WriteDateTime(S7BinaryWriter.MaxDateTime.AddTicks(1))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => writer.WriteDateTime(new DateTime(S7BinaryWriter.MinDateTime.Ticks - 1, DateTimeKind.Utc))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => writer.WriteDateTime(new DateTime(S7BinaryWriter.MinDateTime.Ticks - 1, DateTimeKind.Local))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => writer.WriteDateTime(new DateTime(S7BinaryWriter.MaxDateTime.Ticks + 1, DateTimeKind.Utc))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => writer.WriteDateTime(new DateTime(S7BinaryWriter.MaxDateTime.Ticks + 1, DateTimeKind.Local))).Throws<ArgumentOutOfRangeException>();
    }
}