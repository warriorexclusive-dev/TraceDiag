using System.Net;

namespace TraceDiag.Services;

/// <summary>Reads A and AAAA addresses from a DNS response.</summary>
internal static class DnsAddressAnswer
{
    private const int AddressRecord = 1;
    private const int AaaaRecord = 28;

    internal static IReadOnlyList<IPAddress> Parse(byte[] response, byte[] query, bool ipv6)
    {
        if (!Matches(response, query)) return Array.Empty<IPAddress>();
        var answers = U16(response, 6);
        var offset = SkipQuestions(response, 12, U16(response, 4));
        var result = new List<IPAddress>();
        for (var i = 0; i < answers; i++) offset = ReadAnswer(response, offset, ipv6, result);
        return result;
    }

    private static bool Matches(byte[] response, byte[] query) =>
        response.Length >= 12 && response[0] == query[0] && response[1] == query[1];

    private static int SkipQuestions(byte[] data, int offset, int count)
    {
        for (var i = 0; i < count; i++) offset = DnsNameCursor.Skip(data, offset) + 4;
        return Math.Min(offset, data.Length);
    }

    private static int ReadAnswer(byte[] data, int offset, bool ipv6, List<IPAddress> result)
    {
        offset = DnsNameCursor.Skip(data, offset);
        if (offset + 10 > data.Length) return data.Length;
        var type = U16(data, offset);
        var length = U16(data, offset + 8);
        offset += 10;
        if (offset + length > data.Length) return data.Length;
        AddAddress(data, offset, length, type, ipv6, result);
        return offset + length;
    }

    private static void AddAddress(byte[] data, int offset, int length, int type, bool ipv6, List<IPAddress> result)
    {
        var expected = ipv6 ? AaaaRecord : AddressRecord;
        var size = ipv6 ? 16 : 4;
        if (type != expected || length != size) return;
        var bytes = new byte[size];
        Array.Copy(data, offset, bytes, 0, size);
        result.Add(new IPAddress(bytes));
    }

    private static int U16(byte[] data, int offset) => (data[offset] << 8) | data[offset + 1];
}
