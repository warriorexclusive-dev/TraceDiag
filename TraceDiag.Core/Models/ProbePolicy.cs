namespace TraceDiag.Models;

/// <summary>Resolved packet policy produced by a probe trigger.</summary>
internal sealed record ProbePolicy(int BufferSize, bool DontFragment);
