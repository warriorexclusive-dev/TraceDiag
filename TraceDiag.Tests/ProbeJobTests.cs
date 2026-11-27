using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class ProbeJobTests
{
    [Fact]
    public void DefaultTrigger_KeepsOrdinaryPolicy()
    {
        var policy = new ProbePolicy(512, false);
        Assert.Equal(policy, new DefaultProbeTrigger().Apply(policy));
    }

    [Fact]
    public void DontFragmentTrigger_HasDoNotFragmentIdentity()
    {
        Assert.Equal("DO_NOT_FRAGMENT", DontFragmentProbeTrigger.TriggerName);
    }

    [Fact]
    public void DontFragmentTrigger_EnablesFlagWithoutChangingSize()
    {
        var result = new DontFragmentProbeTrigger().Apply(new ProbePolicy(1400, false));
        Assert.Equal(1400, result.BufferSize);
        Assert.True(result.DontFragment);
    }

    [Fact]
    public void Factory_AppliesTriggerAndKeepsJobImmutable()
    {
        var job = new ProbeJobFactory().Create(new DontFragmentProbeTrigger(),
            TestFactories.Address(1), 64, 1400, 3000, "job");
        Assert.Equal(1400, job.BufferSize);
        Assert.True(job.DontFragment);
        Assert.IsType<DontFragmentProbeTrigger>(job.Trigger);
    }

    [Fact]
    public async Task Runner_SendsThePolicyStoredInTheJob()
    {
        var probe = new FakeProbeClient(call => TestFactories.Success(call));
        var job = new ProbeJobFactory().Create(new DontFragmentProbeTrigger(),
            TestFactories.Address(1), 64, 1024, 3000, "job");
        await new ProbeJobRunner().RunAsync(TestFactories.Context(probe), job,
            TestFactories.TextProgress(), CancellationToken.None);
        var call = Assert.Single(probe.Calls);
        Assert.Equal(1024, call.BufferSize);
        Assert.True(call.DontFragment);
    }
}
