using TraceDiag.Localization;
using TraceDiag.Models;
using TraceDiag.Services;
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
    private readonly ToolTip _tooltips = new();
    private readonly bool _resetRequested;
    private MainControls _controls = null!;
    private StatsGridController _statsGrid = null!;
    private SettingsForm? _settings;

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
    private CheckBox _altDnsInput = null!;
    private CheckBox _asnLookupInput = null!;
    private NumericUpDown _probeTimeoutInput = null!;
    private CheckBox _darkThemeInput = null!;
    private ComboBox _ipFamilyInput = null!;
    private DataGridView _statsGridView = null!;
    private TextBox _outputTextBox = null!;
    private PictureBox _playIcon = null!;
    private PictureBox _stopIcon = null!;
    private PictureBox _settingsIcon = null!;
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
        _settings = new SettingsForm(controls);
        _settings.ApplyConfig(_appConfig);
        _settings.FormClosed += SettingsOnFormClosed;
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
        _playIcon = controls.Play;
        _stopIcon = controls.Stop;
        _settingsIcon = controls.Settings;
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
        _altDnsInput = controls.AlternativeDns;
        _asnLookupInput = controls.AsnLookup;
        _probeTimeoutInput = controls.ProbeTimeout;
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
        ApplyAdapterNote();
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
        _playIcon.Click += RunButtonOnClick;
        _stopIcon.Click += StopIconOnClick;
        _settingsIcon.Click += SettingsIconOnClick;
        SetIconTooltips();
        KeyDown += MainFormOnKeyDown;
    }

    private void SetIconTooltips()
    {
        _tooltips.SetToolTip(_playIcon, Strings.T(Strings.Ui.PlayTooltip));
        _tooltips.SetToolTip(_stopIcon, Strings.T(Strings.Ui.StopTooltip));
        _tooltips.SetToolTip(_settingsIcon, Strings.T(Strings.Ui.SettingsLabel));
    }

    private void StopIconOnClick(object? sender, EventArgs e) => RequestStop();

    private void SettingsIconOnClick(object? sender, EventArgs e) => _settings?.ShowDialog(this);

    private void SettingsOnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (_settings is null) return;
        ApplySettings();
        SaveUiState();
    }

    private void ApplySettings()
    {
        if (_settings is null) return;
        _settings.UpdateConfig(_appConfig);
        _configStore.Save(_appConfig);
        ApplyOutputFontSize((float)_controls.FontSize.Value);
        ApplyLanguage((Language)_controls.Language.SelectedIndex);
        ApplyTheme();
        ApplyAdapterNote();
    }

    private void ApplyAdapterNote()
    {
        var enabled = RawSocketCapability.CanUse();
        _controls.Adapter.Enabled = enabled;
        _controls.AdapterNote.Visible = !enabled;
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
        if (_settings is not null) ThemeApplier.Apply(_settings, palette);
        IconThemeController.Apply(_controls, palette, _appConfig.IconSizePx);
        CaptionThemeController.Apply(this, _appConfig.Theme);
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

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        CaptionThemeController.Apply(this, _appConfig.Theme);
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

}
