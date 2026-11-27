using System.Net;
using System.Net.Sockets;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class NewFeaturesTests
{
    [Theory]
    [InlineData("10.1.2.3", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("172.20.0.1", true)]
    [InlineData("192.168.0.1", true)]
    [InlineData("169.254.1.1", true)]
    [InlineData("fc00::1", true)]
    [InlineData("fe80::1", true)]
    [InlineData("8.8.8.8", false)]
    [InlineData("2001:4860:4860::8888", false)]
    public void PrivateTargets_AreDetectedForExternalSourcePolicy(string text, bool expected) =>
        Assert.Equal(expected, NetworkAddressPolicy.IsPrivateTarget(IPAddress.Parse(text)));

    [Fact]
    public void RawPacket_CreatesValidIcmpEchoRequest()
    {
        var packet = RawIcmpPacket.Create(false, new byte[16], 0x1234, 0x0056);
        Assert.Equal(RawIcmpPacket.EchoRequest4, packet[0]);
        Assert.Equal(0x12, packet[4]);
        Assert.Equal(0x34, packet[5]);
        Assert.Equal(0x00, packet[6]);
        Assert.Equal(0x56, packet[7]);
        Assert.Equal(0xffff, Checksum(packet));
    }

    [Theory]
    [InlineData(RawIcmpPacket.EchoReply4, false, ProbeOutcome.Success)]
    [InlineData(RawIcmpPacket.TimeExceeded4, false, ProbeOutcome.TtlExpired)]
    [InlineData(RawIcmpPacket.Unreachable4, false, ProbeOutcome.DestinationUnreachable)]
    [InlineData(RawIcmpPacket.EchoReply6, true, ProbeOutcome.Success)]
    [InlineData(RawIcmpPacket.TimeExceeded6, true, ProbeOutcome.TtlExpired)]
    public void RawReplies_AreMappedToProbeOutcomes(byte type, bool ipv6, ProbeOutcome expected) =>
        Assert.Equal(expected, RawIcmpReplyMapper.Map(type, IPAddress.Loopback, 5, 32, 1, ipv6).Outcome);

    [Fact]
    public void DnsServerConfig_RoundTripsCustomPath()
    {
        using var temp = new TempDirectory();
        var path = temp.File("custom-dns.json");
        var expected = new[] { IPAddress.Parse("9.9.9.9"), IPAddress.Parse("2001:db8::53") };
        DnsServerConfig.Save(path, expected);
        Assert.Equal(expected, DnsServerConfig.Read(path));
    }

    [Fact]
    public void DnsResolverSelection_UsesCustomAlternativeFile()
    {
        using var temp = new TempDirectory();
        var path = temp.File("custom-dns.json");
        var custom = IPAddress.Parse("9.9.9.9");
        DnsServerConfig.Save(path, [custom]);
        var result = DnsResolverSelection.Get(true, null, path);
        Assert.Contains(custom, result);
        Assert.All(DnsResolverSelection.Get(false), server => Assert.Contains(server, result));
    }

    [Fact]
    public void AdapterCatalog_UnknownIdHasNoSource()
    {
        Assert.Null(NetworkAdapterCatalog.Find("missing-adapter-id-12345"));
        Assert.Null(NetworkAdapterCatalog.Source("missing-adapter-id-12345"));
    }

    [Fact]
    public void Config_RoundTripsIconSizeAndDnsPath()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        ConfigFileIO.Save(path, new AppConfig { IconSizePx = 96, AltDnsFilePath = "C:\\dns\\list.json" });
        var loaded = ConfigFileIO.Load(path);
        Assert.Equal(96, loaded.IconSizePx);
        Assert.Equal("C:\\dns\\list.json", loaded.AltDnsFilePath);
    }

    [Fact]
    public void Config_InvalidIconSizeFallsBackToDefault()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        ConfigFileIO.Save(path, new AppConfig { IconSizePx = 13 });
        Assert.Equal(UiDefaults.DefaultIconSizePx, ConfigFileIO.Load(path).IconSizePx);
    }

    [Fact]
    public void UiState_RoundTripsNewFields()
    {
        using var form = new Form();
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        controls.AsnLookup.Checked = false;
        controls.ProbeTimeout.Value = 1500;
        var state = UiStateCapture.Create(form, controls, new FullScreenState());
        Assert.False(state.AsnLookup);
        Assert.Equal(1500, state.ProbeTimeoutMs);

        controls.AsnLookup.Checked = true;
        controls.ProbeTimeout.Value = 3000;
        UiStateRestorer.Apply(form, controls, state, new AppConfig());
        Assert.False(controls.AsnLookup.Checked);
        Assert.Equal(1500, (int)controls.ProbeTimeout.Value);
    }

    private static int Checksum(byte[] buffer)
    {
        uint sum = 0;
        for (var index = 0; index + 1 < buffer.Length; index += 2)
            sum += (uint)((buffer[index] << 8) + buffer[index + 1]);
        while (sum >> 16 != 0) sum = (sum & 0xffff) + (sum >> 16);
        return (int)sum;
    }
}
