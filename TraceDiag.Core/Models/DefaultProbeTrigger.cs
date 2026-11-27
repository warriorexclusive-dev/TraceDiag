namespace TraceDiag.Models;

/// <summary>Ordinary probe: no fragmentation constraint.</summary>
internal sealed class DefaultProbeTrigger : IProbeTrigger
{
    public ProbePolicy Apply(ProbePolicy policy) => policy;
}
