namespace TraceDiag.Models;

/// <summary>Probe with the IP Don't Fragment flag.</summary>
internal sealed class DontFragmentProbeTrigger : IProbeTrigger
{
    internal const string TriggerName = "DO_NOT_FRAGMENT";

    public ProbePolicy Apply(ProbePolicy policy) => policy with { DontFragment = true };
}
