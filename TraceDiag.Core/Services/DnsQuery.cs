using System.IO;

namespace TraceDiag.Services;

/// <summary>Builds a minimal DNS TXT query packet.</summary>
internal static class DnsQuery
{
    private const int TextRecord = 16;

    internal static byte[] AsText(string name)
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        WriteName(stream, name);
        WriteFooter(stream);
        return stream.ToArray();
    }

    private static void WriteHeader(Stream stream)
    {
        WriteU16(stream, 0x1234);
        WriteU16(stream, 0x0100);
        WriteU16(stream, 1);
        WriteU16(stream, 0);
        WriteU16(stream, 0);
        WriteU16(stream, 0);
    }

    private static void WriteName(Stream stream, string name)
    {
        foreach (var label in name.Split('.'))
        {
            stream.WriteByte((byte)label.Length);
            foreach (var character in label) stream.WriteByte((byte)character);
        }
        stream.WriteByte(0);
    }

    private static void WriteFooter(Stream stream)
    {
        WriteU16(stream, TextRecord);
        WriteU16(stream, 1);
    }

    private static void WriteU16(Stream stream, int value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }
}
