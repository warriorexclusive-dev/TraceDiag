using TraceDiag.Models;

namespace TraceDiag.Ui;

internal sealed record RunInput(
    string Host,
    int MaxPacketSize,
    int Attempts,
    int LossDelayMs,
    bool DynamicPacketSize,
    bool DontFragment = false,
    IpFamily Family = IpFamily.Auto);
