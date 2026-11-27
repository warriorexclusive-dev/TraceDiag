using System.Net;
using System.Text;
using TraceDiag.Services;

namespace TraceDiag.Tests;

public sealed class AsnLookupTests
{
    [Fact]
    public void AsnName_ForIpv4_ReversesOctets()
    {
        Assert.Equal("1.0.0.10.origin.asn.cymru.com", AsnName.For(IPAddress.Parse("10.0.0.1")));
    }

    [Fact]
    public void AsnName_ForIpv6_ReversesNibbles()
    {
        Assert.Equal("1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.origin6.asn.cymru.com",
            AsnName.For(IPAddress.Parse("2001:db8::1")));
    }

    [Fact]
    public void DnsTextAnswer_ParsesFirstAsnField()
    {
        var query = DnsQuery.AsText("1.0.0.10.origin.asn.cymru.com");
        var response = Response(query, "64512 | 10.0.0.0/24 | US | ARIN | 2020-01-01");
        Assert.Equal("64512", DnsTextAnswer.Parse(response, query));
    }

    [Fact]
    public void DnsTextAnswer_RejectsResponseWithDifferentId()
    {
        var query = DnsQuery.AsText("1.0.0.10.origin.asn.cymru.com");
        var response = Response(query, "64512 | 10.0.0.0/24");
        response[0] = 0;
        Assert.Null(DnsTextAnswer.Parse(response, query));
    }

    private static byte[] Response(byte[] query, string text)
    {
        var packet = new List<byte>();
        U16(packet, 0x1234);
        U16(packet, 0x8180);
        U16(packet, 1);
        U16(packet, 1);
        U16(packet, 0);
        U16(packet, 0);
        packet.AddRange(query.Skip(12));
        U16(packet, 0xC00C);
        U16(packet, 16);
        U16(packet, 1);
        U32(packet, 60);
        var txt = Encoding.ASCII.GetBytes(text);
        U16(packet, txt.Length + 1);
        packet.Add((byte)txt.Length);
        packet.AddRange(txt);
        return packet.ToArray();
    }

    private static void U16(List<byte> packet, int value)
    {
        packet.Add((byte)(value >> 8));
        packet.Add((byte)value);
    }

    private static void U32(List<byte> packet, int value)
    {
        U16(packet, value >> 16);
        U16(packet, value);
    }
}
