namespace TraceDiag.Services.Diagnostics;

internal static class PacketSizePlanner
{
    internal static IReadOnlyList<int> Build(int maxPacketSize, bool dynamicPacketSize)
    {
        if (!dynamicPacketSize) return [maxPacketSize];
        var sizes = CollectDoublingSizes(maxPacketSize);
        AppendTarget(maxPacketSize, sizes);
        return sizes;
    }

    private static List<int> CollectDoublingSizes(int maxPacketSize)
    {
        var sizes = new List<int>();
        var size = 64;
        while (size < maxPacketSize)
        {
            sizes.Add(size);
            if (size > maxPacketSize / 2) break;
            size *= 2;
        }
        return sizes;
    }

    private static void AppendTarget(int maxPacketSize, List<int> sizes)
    {
        if (sizes.Count == 0 || sizes[^1] != maxPacketSize) sizes.Add(maxPacketSize);
    }
}
