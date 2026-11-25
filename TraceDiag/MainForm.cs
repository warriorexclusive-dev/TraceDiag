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

    private TextBox _remoteHostTextBox = null!;
    private NumericUpDown _maxPacketSizeInput = null!;
    private NumericUpDown _attemptsInput = null!;
    private NumericUpDown _fontSizeInput = null!;
    private CheckBox _dynamicPacketSizeInput = null!;
    private DataGridView _statsGridView = null!;
    private TextBox _outputTextBox = null!;
    private Button _runButton = null!;
    private Label _statusLabel = null!;

    internal FileDiagnosticLogger Logger => _logger;
    internal Label StatusLabel => _statusLabel;

    public MainForm()
    {
        var appDataDir = AppFiles.GetAppDataDirectory();
        _configStore = AppFiles.CreateConfigStore(appDataDir);
        _appConfig = _configStore.Load();
        _outputFontSize = _appConfig.OutputFontSize;
        _logger = AppFiles.CreateLogger(appDataDir);
        var controls = FormControlsFactory.Create(_outputFontSize);
        AssignControls(controls);
        ConfigureWindow();
        SubscribeEvents();
        Controls.Add(controls.Root);
        LogStartup();
    }

    private void AssignControls(MainControls controls)
    {
        _remoteHostTextBox = controls.RemoteHost;
        _maxPacketSizeInput = controls.MaxPacketSize;
        _attemptsInput = controls.Attempts;
        _fontSizeInput = controls.FontSize;
        _dynamicPacketSizeInput = controls.DynamicPacketSize;
        _statsGridView = controls.Stats;
        _outputTextBox = controls.Output;
        _runButton = controls.Run;
        _statusLabel = controls.Status;
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
        Icon = SystemIconLoader.TryLoadNetworkIcon() ?? SystemIcons.Application;
    }

    private void SubscribeEvents()
    {
        _fontSizeInput.ValueChanged += FontSizeInputOnValueChanged;
        _runButton.Click += RunButtonOnClick;
        KeyDown += MainFormOnKeyDown;
    }

    private void LogStartup()
    {
        _logger.Info(Strings.Log.ApplicationStarted);
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleLogFileFormat, TextFormatter.Now(), _logger.LogFilePath));
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleConfigFileFormat, TextFormatter.Now(), _configStore.ConfigFilePath));
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

    private void MainFormOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (FullScreenController.HandleKey(this, e, _fullScreen)) e.Handled = true;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _runCts?.Cancel();
        base.OnFormClosing(e);
    }

    private async void RunButtonOnClick(object? sender, EventArgs e)
    {
        if (_runCts is not null) return;
        _runCts = new CancellationTokenSource();
        await RunFlow.StartAsync(new RunFlowContext(this, _runCts.Token));
    }

    internal RunInput? ReadRunInput()
    {
        var host = _remoteHostTextBox.Text.Trim();
        _remoteHostTextBox.Text = host;
        if (!string.IsNullOrWhiteSpace(host)) return new RunInput(host, (int)_maxPacketSizeInput.Value, (int)_attemptsInput.Value, _dynamicPacketSizeInput.Checked);
        MessageBox.Show(this, Strings.Ui.RemoteHostRequired, Strings.Ui.ValidationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _remoteHostTextBox.Focus();
        return null;
    }

    internal void BeginRun()
    {
        _outputTextBox.Clear();
        ClearStats();
        AppendOutputLine(TextFormatter.Fmt(Strings.App.ConsoleLogFileFormat, TextFormatter.Now(), _logger.LogFilePath));
        _statusLabel.Text = Strings.Ui.ResolvingHost;
        _runButton.Enabled = false;
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
        _runButton.Enabled = true;
        SetInputsEnabled(true);
    }

    private void SetInputsEnabled(bool enabled)
    {
        _remoteHostTextBox.Enabled = enabled;
        _maxPacketSizeInput.Enabled = enabled;
        _attemptsInput.Enabled = enabled;
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
