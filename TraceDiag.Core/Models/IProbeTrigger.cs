namespace TraceDiag.Models;

/// <summary>Algorithm-independent trigger attached to a probe job.</summary>
internal interface IProbeTrigger
{
    ProbePolicy Apply(ProbePolicy policy);
}
