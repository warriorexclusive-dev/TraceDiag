using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

public interface IMonitoringService
{
    Task RunAsync(
        MonitoringInput input,
        IProgress<string> progress,
        IProgress<HopStatSnapshot> statsProgress,
        CancellationToken cancellationToken);
}
