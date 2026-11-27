namespace TraceDiag.Services;

/// <summary>Skips DNS names, including compressed pointers.</summary>
internal static class DnsNameCursor
{
    internal static int Skip(byte[] data, int offset)
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
        (Target(data, offset), End(data, offset, jumped, end), true);

    private static bool IsPointer(byte length) => (length & 0xC0) == 0xC0;

    private static int End(byte[] data, int offset, bool jumped, int end) =>
        jumped ? end : Math.Min(offset + 2, data.Length);

    private static int Target(byte[] data, int offset) =>
        offset + 1 >= data.Length ? data.Length : ((data[offset] & 0x3F) << 8) | data[offset + 1];
}
