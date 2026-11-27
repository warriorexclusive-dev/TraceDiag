using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Maps mode flags to reusable trigger strategies.</summary>
internal static class ProbeTriggers
{
    internal static IProbeTrigger Create(bool dontFragment) =>
        dontFragment ? new DontFragmentProbeTrigger() : new DefaultProbeTrigger();
}
