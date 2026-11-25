namespace TraceDiag.Ui;

internal sealed record RunInput(string Host, int MaxPacketSize, int Attempts, bool DynamicPacketSize);
