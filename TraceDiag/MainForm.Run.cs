using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Services.Monitoring;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed partial class MainForm
{
    private async void RunButtonOnClick(object? sender, EventArgs e)
    {
        if (_runCts is not null) return;
        _runCts = new CancellationTokenSource();
        if (_monitoringModeInput.Checked) await MonitorFlow.StartAsync(new MonitorFlowContext(this, _runCts.Token));
        else await RunFlow.StartAsync(new RunFlowContext(this, _runCts.Token));
    }

    private bool RequestStop()
    {
        if (_runCts is null) return false;
        _runCts.Cancel();
        UiText.Set(_statusLabel, Strings.Ui.Stopping);
        return true;
    }

    internal void BeginRun() => Begin(Strings.Ui.ResolvingHost);

    internal void BeginMonitor() => Begin(Strings.Ui.Monitoring);

    private void Begin(string status)
    {
        _outputBuffer.Clear();
        _outputTextBox.Clear();
        _statistics.Clear();
        _statsGrid.Render(Array.Empty<HopStatSnapshot>());
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleLogFileFormat, TextFormatter.Now(), _logger.LogFilePath));
        UiText.Set(_statusLabel, status);
        IconThemeController.Running(_controls, true);
        ApplyInputsEnabled();
    }

    internal Task ExecuteServiceAsync(DiagnosticInput input, IProgress<string> text,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken token)
    {
        var adapter = AdapterList.SelectedId(_controls.Adapter);
        var service = new TraceDiagnosticsService(IcmpProbeClientFactory.Create(adapter),
            new DnsResolverService(adapter), _logger, new RipeOwnerService(), adapter, DnsFilePath());
        return service.RunAsync(input, text, stats, token);
    }

    internal Task ExecuteMonitorAsync(MonitoringInput input, IProgress<string> text,
        IProgress<HopStatSnapshot> stats, CancellationToken token)
    {
        var adapter = AdapterList.SelectedId(_controls.Adapter);
        var service = new MonitoringService(IcmpProbeClientFactory.Create(adapter),
            new DnsResolverService(adapter), _logger, new RipeOwnerService(), adapter, DnsFilePath());
        return service.RunAsync(input, text, stats, token);
    }

    private string DnsFilePath() => string.IsNullOrWhiteSpace(_appConfig.AltDnsFilePath)
        ? DnsPathPolicy.Default() : _appConfig.AltDnsFilePath;

    internal void EndRun()
    {
        _runCts?.Dispose();
        _runCts = null;
        IconThemeController.Running(_controls, false);
        ApplyInputsEnabled();
    }

    private void ApplyInputsEnabled() => _packetPolicy.Refresh();

    internal void ApplyStats(IReadOnlyList<HopStatSnapshot> stats) => _statistics.Replace(stats);

    internal void ApplyMonitorStat(HopStatSnapshot stat) => _statistics.Upsert(stat);

    private void RenderStats(IReadOnlyList<HopStatSnapshot> stats) =>
        UiInvoker.OnUiThread(_statsGridView, () => _statsGrid.Render(stats));

    internal void AppendOutputLine(string line) =>
        UiInvoker.OnUiThread(_outputTextBox, () => _outputBuffer.Append(_outputTextBox, line));
}
