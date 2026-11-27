using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class DiagnosisTextTests
{
    [Fact]
    public void Localize_TranslatesEveryReason()
    {
        using var localization = new LocalizationDirectory().Add("ru",
            (Strings.Diagnosis.PacketSizeLimit, "Обрезка ICMP"),
            (Strings.Diagnosis.EcmpBalancing, "ECMP-балансировка"));
        localization.Activate();
        var canonical = string.Join(Strings.Diagnosis.Separator,
            Strings.Diagnosis.PacketSizeLimit, Strings.Diagnosis.EcmpBalancing);
        Assert.Equal("Обрезка ICMP" + Strings.Diagnosis.Separator + "ECMP-балансировка",
            DiagnosisText.Localize(canonical));
    }

    [Fact]
    public void Contains_FindsReasonInsideCombinedText()
    {
        var canonical = string.Join(Strings.Diagnosis.Separator,
            Strings.Diagnosis.PacketSizeLimit, Strings.Diagnosis.AntiFlood,
            Strings.Diagnosis.EcmpBalancing);
        Assert.True(DiagnosisText.Contains(canonical, Strings.Diagnosis.EcmpBalancing));
        Assert.False(DiagnosisText.Contains(canonical, Strings.Diagnosis.RouteChange));
    }
}
