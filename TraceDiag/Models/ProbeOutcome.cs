namespace TraceDiag.Models;

public enum ProbeOutcome
{
    Success,
    TimedOut,
    TtlExpired,
    DestinationUnreachable,
    Failed
}
