namespace TraceDiag.Ui;

internal sealed record RunInput(string Host, int MaxPacketSize, int Attempts, int LossDelayMs, bool DynamicPacketSize);
