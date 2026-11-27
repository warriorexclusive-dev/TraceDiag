using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Services.Monitoring;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed partial class MainForm : Form
{
    private readonly FullScreenState _fullScreen = new();
    private CancellationTokenSource? _runCts;
    private float _outputFontSize;
    private int _statsPercent = UiDefaults.DefaultStatsPercent;
    private readonly AppConfigStore _configStore;
    private readonly AppConfig _appConfig;
    private readonly FileDiagnosticLogger _logger;
    private readonly UiStateStore _uiStateStore;
    private readonly StatisticsController _statistics;
    private readonly IPacketSizePolicy _packetPolicy;
    private readonly OutputBufferController _outputBuffer = new();
    private readonly bool _resetRequested;
    private MainControls _controls = null!;
    private StatsGridController _statsGrid = null!;

    private TextBox _remoteHostTextBox = null!;
    private NumericUpDown _maxPacketSizeInput = null!;
    private NumericUpDown _attemptsInput = null!;
    private NumericUpDown _lossDelayInput = null!;
    private NumericUpDown _fontSizeInput = null!;
    private CheckBox _dynamicPacketSizeInput = null!;
    private CheckBox _dontFragmentInput = null!;
    private CheckBox _monitoringModeInput = null!;
    private CheckBox _ripeLookupInput = null!;
    private CheckBox _routePrecheckInput = null!;
    private CheckBox _deepRoutesInput = null!;
    private CheckBox _darkThemeInput = null!;
    private ComboBox _ipFamilyInput = null!;
    private DataGridView _statsGridView = null!;
    private TextBox _outputTextBox = null!;
    private Button _runButton = null!;
    private Label _statusLabel = null!;

    internal FileDiagnosticLogger Logger => _logger;
    internal Label StatusLabel => _statusLabel;

    public MainForm() : this(false, null) { }

    internal MainForm(bool resetUiState) : this(resetUiState, null) { }

    internal MainForm(bool resetUiState, UiStateStore? uiStateStore)
    {
        (_configStore, _appConfig, _logger, _uiStateStore) = CreateServices(resetUiState, uiStateStore);
        Translator.SetLanguage(_appConfig.Language);
        _resetRequested = resetUiState;
        _statistics = new StatisticsController(publish: RenderStats);
        var loadResult = _uiStateStore.Load();
        var state = loadResult.State;
        _outputFontSize = state?.OutputFontSize ?? _appConfig.OutputFontSize;
        _statsPercent = state?.StatsHeightPercent ?? UiDefaults.DefaultStatsPercent;
        var darkTheme = state?.DarkTheme ?? _appConfig.Theme == AppTheme.Dark;
        var controls = FormControlsFactory.Create(_outputFontSize, darkTheme);
        AssignControls(controls);
        _packetPolicy = new PacketSizePolicyController(controls, () => _runCts is null);
        ConfigureWindow();
        ApplyLoadedState(loadResult);
        FinishInitialization(controls, loadResult);
    }

    private static (AppConfigStore Store, AppConfig Config, FileDiagnosticLogger Logger, UiStateStore UiStateStore) CreateServices(bool resetUiState, UiStateStore? provided)
    {
        var appDataDir = AppFiles.GetAppDataDirectory();
        var stateStore = provided ?? AppFiles.CreateUiStateStore(appDataDir);
        if (resetUiState) stateStore.Reset();
        var configStore = AppFiles.CreateConfigStore(appDataDir);
        return (configStore, configStore.Load(), AppFiles.CreateLogger(appDataDir), stateStore);
    }

    private void AssignControls(MainControls controls)
    {
        _controls = controls;
        AssignInputs(controls);
        _statsGridView = controls.Stats;
        _statsGrid = new StatsGridController(_statsGridView);
        _outputTextBox = controls.Output;
        _runButton = controls.Run;
        _statusLabel = controls.Status;
    }

    private void AssignInputs(MainControls controls)
    {
        _remoteHostTextBox = controls.RemoteHost;
        _maxPacketSizeInput = controls.MaxPacketSize;
        _attemptsInput = controls.Attempts;
        _lossDelayInput = controls.LossDelay;
        _fontSizeInput = controls.FontSize;
        _dynamicPacketSizeInput = controls.DynamicPacketSize;
        _dontFragmentInput = controls.DontFragment;
        _monitoringModeInput = controls.MonitoringMode;
        _ripeLookupInput = controls.RipeLookup;
        _routePrecheckInput = controls.RoutePrecheck;
        _deepRoutesInput = controls.DeepRoutes;
        _darkThemeInput = controls.DarkTheme;
        _ipFamilyInput = controls.IpFamily;
    }

    private void FinishInitialization(MainControls controls, UiStateLoadResult loadResult)
    {
        SubscribeEvents();
        _packetPolicy.Refresh();
        Controls.Add(controls.Root);
        InitializeLanguage();
        ApplyTheme();
        LogStartup(loadResult);
    }

    private void ApplyLoadedState(UiStateLoadResult loadResult)
    {
        if (loadResult.State is null) return;
        UiStateRestorer.Apply(this, _controls, loadResult.State, _appConfig);
        _configStore.Save(_appConfig);
    }

    private void SaveUiState()
    {
        var state = UiStateCapture.Create(this, _controls, _fullScreen);
        if (_uiStateStore.Save(state)) _logger.Info(Strings.Log.UiStateSaved);
        else _logger.Warn(Strings.Log.UiStateSaveFailed);
    }

    private void ConfigureWindow()
    {
        Text = Strings.App.Title;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = MinimizeBox = true;
        MinimumSize = ClientSize = new Size(800, 600);
        KeyPreview = true;
        Icon = AppIconLoader.Load();
    }

    private void SubscribeEvents()
    {
        SubscribeLanguage();
        _fontSizeInput.ValueChanged += FontSizeInputOnValueChanged;
        _darkThemeInput.CheckedChanged += DarkThemeInputOnCheckedChanged;
        _monitoringModeInput.CheckedChanged += MonitoringModeOnCheckedChanged;
        _dontFragmentInput.CheckedChanged += DontFragmentOnCheckedChanged;
        _runButton.Click += RunButtonOnClick;
        KeyDown += MainFormOnKeyDown;
    }

    private void LogStartup(UiStateLoadResult loadResult)
    {
        _logger.Info(Strings.Log.ApplicationStarted);
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleLogFileFormat, TextFormatter.Now(), _logger.LogFilePath));
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleConfigFileFormat, TextFormatter.Now(), _configStore.ConfigFilePath));
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleUiStateFileFormat, TextFormatter.Now(), _uiStateStore.FilePath));
        if (_resetRequested) _logger.Info(Strings.Log.UiStateReset);
        if (loadResult.Corrupted) _logger.Warn(Strings.Log.UiStateCorruptedReset);
        if (loadResult.State is not null) _logger.Info(Strings.Log.UiStateLoaded);
    }

    private void FontSizeInputOnValueChanged(object? sender, EventArgs e)
    {
        ApplyOutputFontSize((float)_fontSizeInput.Value);
        _appConfig.OutputFontSize = _outputFontSize;
        _configStore.Save(_appConfig);
        _logger.Info(TextFormatter.Fmt(Strings.Log.SettingsSavedFormat, _outputFontSize));
    }

    private void ApplyOutputFontSize(float fontSize)
    {
        _outputFontSize = (float)UiDefaults.ClampFontSize(fontSize);
        var previous = _outputTextBox.Font;
        _outputTextBox.Font = new Font(previous.FontFamily, _outputFontSize, previous.Style);
        previous.Dispose();
    }

    private void ApplyTheme()
    {
        var palette = ThemeApplier.Palette(_appConfig.Theme);
        ThemeApplier.Apply(this, palette);
        _statsGrid.SetPalette(palette);
    }

    private void DarkThemeInputOnCheckedChanged(object? sender, EventArgs e)
    {
        _appConfig.Theme = _darkThemeInput.Checked ? AppTheme.Dark : AppTheme.Light;
        _configStore.Save(_appConfig);
        ApplyTheme();
        _logger.Info(TextFormatter.Fmt(Strings.Log.ThemeChangedFormat, _appConfig.Theme));
    }

    private void MonitoringModeOnCheckedChanged(object? sender, EventArgs e) => ApplyInputsEnabled();

    private void DontFragmentOnCheckedChanged(object? sender, EventArgs e) => ApplyInputsEnabled();

    private void MainFormOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (FullScreenController.HandleKey(this, e, _fullScreen)) e.Handled = true;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        SplitterController.Apply(_controls.Splitter, _statsPercent);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _runCts?.Cancel();
        SaveUiState();
        _statistics.Flush();
        _statistics.Dispose();
        _logger.Flush();
        _logger.Dispose();
        base.OnFormClosing(e);
    }

    private async void RunButtonOnClick(object? sender, EventArgs e)
    {
        if (RequestStop()) return;
        _runCts = new CancellationTokenSource();
        if (_monitoringModeInput.Checked) await MonitorFlow.StartAsync(new MonitorFlowContext(this, _runCts.Token));
        else await RunFlow.StartAsync(new RunFlowContext(this, _runCts.Token));
    }

    private bool RequestStop()
    {
        if (_runCts is null) return false;
        _runCts.Cancel();
        UiText.Set(_statusLabel, Strings.Ui.Stopping);
        RunButtonState.Stopping(_runButton);
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
        RunButtonState.Begin(_runButton);
        ApplyInputsEnabled();
    }

    internal Task ExecuteServiceAsync(DiagnosticInput input, IProgress<string> text, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken token)
    {
        var service = new TraceDiagnosticsService(new IcmpProbeClient(), new DnsResolverService(),
            _logger, new RipeOwnerService());
        return service.RunAsync(input, text, stats, token);
    }

    internal Task ExecuteMonitorAsync(MonitoringInput input, IProgress<string> text, IProgress<HopStatSnapshot> stats, CancellationToken token)
    {
        var service = new MonitoringService(new IcmpProbeClient(), new DnsResolverService(),
            _logger, new RipeOwnerService());
        return service.RunAsync(input, text, stats, token);
    }

    internal void EndRun()
    {
        _runCts?.Dispose();
        _runCts = null;
        RunButtonState.End(_runButton);
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
