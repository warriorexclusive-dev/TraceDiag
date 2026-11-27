using System.Text;

namespace TraceDiag.Services;

/// <summary>Reads the first ASN field from a Team Cymru DNS TXT response.</summary>
internal static class DnsTextAnswer
{
    private const int TextRecord = 16;

    internal static string? Parse(byte[] response, byte[] query)
    {
        if (response.Length < 12 || response[0] != query[0] || response[1] != query[1]) return null;
        var questions = U16(response, 4);
        var answers = U16(response, 6);
        var offset = 12;
        for (var i = 0; i < questions; i++) offset = SkipQuestion(response, offset);
        for (var i = 0; i < answers; i++)
        {
            var asn = ReadAnswer(response, ref offset);
            if (asn is not null) return asn;
        }
        return null;
    }

    private static int SkipQuestion(byte[] data, int offset) => SkipName(data, offset) + 4;

    private static string? ReadAnswer(byte[] data, ref int offset)
    {
        offset = SkipName(data, offset);
        if (offset + 10 > data.Length) return null;
        var type = U16(data, offset);
        var length = U16(data, offset + 8);
        offset += 10;
        if (offset + length > data.Length) return null;
        var text = type == TextRecord ? ReadText(data, offset, length) : string.Empty;
        offset += length;
        return FirstField(text);
    }

    private static string ReadText(byte[] data, int offset, int length)
    {
        var text = new StringBuilder();
        var end = offset + length;
        while (offset < end)
        {
            var size = data[offset++];
            var count = Math.Min(size, end - offset);
            text.Append(Encoding.ASCII.GetString(data, offset, count));
            offset += count;
        }
        return text.ToString();
    }

    private static string? FirstField(string text)
    {
        var value = text.Split('|')[0].Trim();
        return value.Length == 0 ? null : value;
    }

    private static int SkipName(byte[] data, int offset)
    {
        var end = offset;
        var jumped = false;
        while (offset < data.Length && data[offset] != 0)
        {
            if (!IsPointer(data[offset])) { offset += 1 + data[offset]; continue; }
            (offset, end, jumped) = Follow(data, offset, end, jumped);
        }
        return jumped ? end : Math.Min(offset + 1, data.Length);
    }

    private static (int Offset, int End, bool Jumped) Follow(byte[] data, int offset, int end, bool jumped) =>
        (PointerTarget(data, offset, data[offset]), PointerEnd(data, offset, jumped, end), true);

    private static bool IsPointer(byte length) => (length & 0xC0) == 0xC0;

    private static int PointerEnd(byte[] data, int offset, bool jumped, int end) =>
        jumped ? end : Math.Min(offset + 2, data.Length);

    private static int PointerTarget(byte[] data, int offset, byte length) =>
        offset + 1 >= data.Length ? data.Length : ((length & 0x3F) << 8) | data[offset + 1];

    private static int U16(byte[] data, int offset) => (data[offset] << 8) | data[offset + 1];
}
