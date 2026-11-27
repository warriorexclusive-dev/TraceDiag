namespace TraceDiag.Services;

/// <summary>Builds ICMP echo requests and validates replies for raw sockets.</summary>
internal static class RawIcmpPacket
{
    internal const byte EchoRequest4 = 8;
    internal const byte EchoReply4 = 0;
    internal const byte EchoRequest6 = 128;
    internal const byte EchoReply6 = 129;
    internal const byte TimeExceeded4 = 11;
    internal const byte TimeExceeded6 = 3;
    internal const byte Unreachable4 = 3;
    internal const byte Unreachable6 = 1;

    internal static byte[] Create(bool ipv6, byte[] payload, int identifier, int sequence)
    {
        var packet = new byte[8 + payload.Length];
        packet[0] = ipv6 ? EchoRequest6 : EchoRequest4;
        WriteU16(packet, 4, identifier);
        WriteU16(packet, 6, sequence);
        Buffer.BlockCopy(payload, 0, packet, 8, payload.Length);
        if (!ipv6) WriteU16(packet, 2, Checksum(packet));
        return packet;
    }

    internal static int HeaderOffset(byte[] data) =>
        data.Length > 20 && data[0] >> 4 == 4 ? (data[0] & 0x0f) * 4 : 0;

    private static void WriteU16(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }

    private static ushort Checksum(byte[] buffer)
    {
        uint sum = 0;
        for (var index = 0; index + 1 < buffer.Length; index += 2)
            sum += (uint)((buffer[index] << 8) + buffer[index + 1]);
        if (buffer.Length % 2 == 1) sum += (uint)(buffer[buffer.Length - 1] << 8);
        while (sum >> 16 != 0) sum = (sum & 0xffff) + (sum >> 16);
        return (ushort)~sum;
    }
}
