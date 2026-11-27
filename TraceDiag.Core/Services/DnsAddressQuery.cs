using System.IO;

namespace TraceDiag.Services;

/// <summary>Builds minimal DNS A and AAAA query packets.</summary>
internal static class DnsAddressQuery
{
    private const int AddressRecord = 1;
    private const int AaaaRecord = 28;

    internal static byte[] Create(string name, bool ipv6)
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        WriteName(stream, name);
        WriteFooter(stream, ipv6 ? AaaaRecord : AddressRecord);
        return stream.ToArray();
    }

    private static void WriteHeader(Stream stream)
    {
        WriteU16(stream, 0x4321);
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

    private static void WriteFooter(Stream stream, int type)
    {
        WriteU16(stream, type);
        WriteU16(stream, 1);
    }

    private static void WriteU16(Stream stream, int value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }
}
