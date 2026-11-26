using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed class MainForm : Form
{
    private readonly List<HopStatSnapshot> _currentStats = new();
    private readonly FullScreenState _fullScreen = new();
    private CancellationTokenSource? _runCts;
    private float _outputFontSize;
    private readonly AppConfigStore _configStore;
    private readonly AppConfig _appConfig;
    private readonly FileDiagnosticLogger _logger;
    private readonly UiStateStore _uiStateStore;
    private readonly bool _resetRequested;
    private MainControls _controls = null!;

    private TextBox _remoteHostTextBox = null!;
    private NumericUpDown _maxPacketSizeInput = null!;
    private NumericUpDown _attemptsInput = null!;
    private NumericUpDown _lossDelayInput = null!;
    private NumericUpDown _fontSizeInput = null!;
    private CheckBox _dynamicPacketSizeInput = null!;
    private CheckBox _darkThemeInput = null!;
    private DataGridView _statsGridView = null!;
    private TextBox _outputTextBox = null!;
    private Button _runButton = null!;
    private Label _statusLabel = null!;

    internal FileDiagnosticLogger Logger => _logger;
    internal Label StatusLabel => _statusLabel;

    public MainForm() : this(false, null)
    {
    }

    internal MainForm(bool resetUiState) : this(resetUiState, null)
    {
    }

    internal MainForm(bool resetUiState, UiStateStore? uiStateStore)
    {
        (_configStore, _appConfig, _logger, _uiStateStore) = CreateServices(resetUiState, uiStateStore);
        _resetRequested = resetUiState;
        var loadResult = _uiStateStore.Load();
        var state = loadResult.State;
        _outputFontSize = state?.OutputFontSize ?? _appConfig.OutputFontSize;
        var darkTheme = state?.DarkTheme ?? _appConfig.Theme == AppTheme.Dark;
        var controls = FormControlsFactory.Create(_outputFontSize, darkTheme);
        AssignControls(controls);
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
        _remoteHostTextBox = controls.RemoteHost;
        _maxPacketSizeInput = controls.MaxPacketSize;
        _attemptsInput = controls.Attempts;
        _lossDelayInput = controls.LossDelay;
        _fontSizeInput = controls.FontSize;
        _dynamicPacketSizeInput = controls.DynamicPacketSize;
        _darkThemeInput = controls.DarkTheme;
        _statsGridView = controls.Stats;
        _outputTextBox = controls.Output;
        _runButton = controls.Run;
        _statusLabel = controls.Status;
    }

    private void FinishInitialization(MainControls controls, UiStateLoadResult loadResult)
    {
        SubscribeEvents();
        Controls.Add(controls.Root);
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
        MaximizeBox = true;
        MinimizeBox = true;
        ClientSize = new Size(800, 600);
        MinimumSize = new Size(800, 600);
        KeyPreview = true;
        Icon = AppIconLoader.Load();
    }

    private void SubscribeEvents()
    {
        _fontSizeInput.ValueChanged += FontSizeInputOnValueChanged;
        _darkThemeInput.CheckedChanged += DarkThemeInputOnCheckedChanged;
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

    private void ApplyTheme() => ThemeApplier.Apply(this, _appConfig.Theme);

    private void DarkThemeInputOnCheckedChanged(object? sender, EventArgs e)
    {
        _appConfig.Theme = _darkThemeInput.Checked ? AppTheme.Dark : AppTheme.Light;
        _configStore.Save(_appConfig);
        ApplyTheme();
        _logger.Info(TextFormatter.Fmt(Strings.Log.ThemeChangedFormat, _appConfig.Theme));
    }

    private void MainFormOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (FullScreenController.HandleKey(this, e, _fullScreen)) e.Handled = true;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _runCts?.Cancel();
        SaveUiState();
        base.OnFormClosing(e);
    }

    private async void RunButtonOnClick(object? sender, EventArgs e)
    {
        if (RequestStop()) return;
        _runCts = new CancellationTokenSource();
        await RunFlow.StartAsync(new RunFlowContext(this, _runCts.Token));
    }

    private bool RequestStop()
    {
        if (_runCts is null) return false;
        _runCts.Cancel();
        _statusLabel.Text = Strings.Ui.Stopping;
        RunButtonState.Stopping(_runButton);
        return true;
    }

    internal RunInput? ReadRunInput()
    {
        var host = _remoteHostTextBox.Text.Trim();
        _remoteHostTextBox.Text = host;
        if (!string.IsNullOrWhiteSpace(host)) return CreateRunInput(host);
        MessageBox.Show(this, Strings.Ui.RemoteHostRequired, Strings.Ui.ValidationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _remoteHostTextBox.Focus();
        return null;
    }

    private RunInput CreateRunInput(string host) =>
        new(host, (int)_maxPacketSizeInput.Value, (int)_attemptsInput.Value,
            (int)_lossDelayInput.Value, _dynamicPacketSizeInput.Checked);

    internal void BeginRun()
    {
        _outputTextBox.Clear();
        ClearStats();
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleLogFileFormat, TextFormatter.Now(), _logger.LogFilePath));
        _statusLabel.Text = Strings.Ui.ResolvingHost;
        RunButtonState.Begin(_runButton);
        SetInputsEnabled(false);
    }

    internal Task ExecuteServiceAsync(DiagnosticInput input, IProgress<string> text, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken token)
    {
        var service = new TraceDiagnosticsService(new IcmpProbeClient(), new DnsResolverService(), _logger);
        return service.RunAsync(input, text, stats, token);
    }

    internal void EndRun()
    {
        _runCts?.Dispose();
        _runCts = null;
        RunButtonState.End(_runButton);
        SetInputsEnabled(true);
    }

    private void SetInputsEnabled(bool enabled)
    {
        _remoteHostTextBox.Enabled = enabled;
        _maxPacketSizeInput.Enabled = enabled;
        _attemptsInput.Enabled = enabled;
        _lossDelayInput.Enabled = enabled;
        _dynamicPacketSizeInput.Enabled = enabled;
    }

    private void ClearStats()
    {
        _currentStats.Clear();
        UiInvoker.OnUiThread(_statsGridView, RefreshStatsGrid);
    }

    internal void ApplyStats(IReadOnlyList<HopStatSnapshot> stats)
    {
        UiInvoker.OnUiThread(_statsGridView, () =>
        {
            _currentStats.Clear();
            _currentStats.AddRange(stats);
            RefreshStatsGrid();
        });
    }

    private void RefreshStatsGrid()
    {
        if (IsDisposed) return;
        _statsGridView.SuspendLayout();
        _statsGridView.Rows.Clear();
        foreach (var stat in _currentStats) StatsGridRows.AddRow(_statsGridView, stat);
        _statsGridView.ResumeLayout();
    }

    internal void AppendOutputLine(string line)
    {
        UiInvoker.OnUiThread(_outputTextBox, () => _outputTextBox.AppendText(line + Environment.NewLine));
    }
}
