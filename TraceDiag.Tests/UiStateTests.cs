using System.Windows.Forms;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class UiStateTests
{
    [Fact]
    public void ResetRequested_AcceptsPlainAndPrefixedArguments()
    {
        Assert.True(Program.ResetRequested(["reset"]));
        Assert.True(Program.ResetRequested(["--reset"]));
        Assert.True(Program.ResetRequested(["/RESET"]));
        Assert.False(Program.ResetRequested(["run"]));
    }

    [Fact]
    public void Store_SaveThenLoad_PreservesState()
    {
        using var temp = new TempDirectory();
        var store = new UiStateStore(temp.File("ui-state.json"));
        var state = CreateState();
        state.StatsHeightPercent = 62;
        Assert.True(store.Save(state));
        var result = store.Load();
        Assert.False(result.Corrupted);
        Assert.NotNull(result.State);
        Assert.Equal(state.RemoteHost, result.State!.RemoteHost);
        Assert.Equal(state.MonitoringMode, result.State.MonitoringMode);
        Assert.Equal(state.DontFragment, result.State.DontFragment);
        Assert.Equal(state.IpFamily, result.State.IpFamily);
        Assert.Equal(state.ColumnWidths[Strings.Ui.ColumnKeyIp], result.State.ColumnWidths[Strings.Ui.ColumnKeyIp]);
        Assert.Equal(62, result.State.StatsHeightPercent);
    }

    [Fact]
    public void Store_Load_WhenFileMissing_ReturnsNullWithoutError()
    {
        using var temp = new TempDirectory();
        var result = new UiStateStore(temp.File("missing.json")).Load();
        Assert.Null(result.State);
        Assert.False(result.Corrupted);
    }

    [Fact]
    public void Store_Load_WithCorruptedJson_DeletesFile()
    {
        using var temp = new TempDirectory();
        var path = temp.File("ui-state.json");
        File.WriteAllText(path, "{not json");
        var result = new UiStateStore(path).Load();
        Assert.Null(result.State);
        Assert.True(result.Corrupted);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Store_Load_WithInvalidValues_DeletesFile()
    {
        using var temp = new TempDirectory();
        var path = temp.File("ui-state.json");
        File.WriteAllText(path, InvalidStateJson());
        var result = new UiStateStore(path).Load();
        Assert.Null(result.State);
        Assert.True(result.Corrupted);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Validator_RejectsUnknownIpFamily()
    {
        var state = CreateState();
        state.IpFamily = 3;
        Assert.False(UiStateValidator.IsValid(state));
    }

    [Fact]
    public void Store_Reset_DeletesExistingFile()
    {
        using var temp = new TempDirectory();
        var path = temp.File("ui-state.json");
        new UiStateStore(path).Save(CreateState());
        Assert.True(File.Exists(path));
        new UiStateStore(path).Reset();
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Capture_ReadsInputsWindowAndColumnWidths()
    {
        using var form = CreateForm();
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        FillControls(controls);
        controls.Root.Size = new Size(900, 700);
        controls.Splitter.Size = new Size(600, 400);
        SetColumnWidth(controls.Stats, Strings.Ui.ColumnKeySent, 123);
        SplitterController.Apply(controls.Splitter, 55);
        var state = UiStateCapture.Create(form, controls, new FullScreenState());
        AssertCapturedInputs(state);
        AssertCapturedBounds(state.Bounds);
        Assert.Equal(123, state.ColumnWidths[Strings.Ui.ColumnKeySent]);
        Assert.Equal(55, state.StatsHeightPercent);
    }

    [Fact]
    public void Restore_AppliesInputsColumnsAndConfig()
    {
        using var form = new Form();
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        var config = new AppConfig();
        controls.Root.Size = new Size(900, 700);
        controls.Splitter.Size = new Size(600, 400);
        UiStateRestorer.Apply(form, controls, CreateState(), config);
        AssertRestoredInputs(controls);
        Assert.Equal(60, SplitterController.Percent(controls.Splitter));
        Assert.Equal(222, controls.Stats.Columns[Strings.Ui.ColumnKeyIp]!.Width);
        Assert.Equal(AppTheme.Light, config.Theme);
        Assert.Equal(11.5f, config.OutputFontSize);
    }

    [Fact]
    public void Restore_WithDontFragment_ClampsSavedSizeAndDynamic()
    {
        using var form = new Form();
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        var state = CreateState();
        state.MaxPacketSize = 2048;
        state.DynamicPacketSize = true;
        UiStateRestorer.Apply(form, controls, state, new AppConfig());
        Assert.Equal(1024, (int)controls.MaxPacketSize.Value);
        Assert.Equal(1024, (int)controls.MaxPacketSize.Maximum);
        Assert.False(controls.DynamicPacketSize.Checked);
    }

    [Fact]
    public void Restore_WithMaximizedWindowState_SetsMaximized()
    {
        using var form = new Form();
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        var state = CreateState();
        state.WindowState = nameof(FormWindowState.Maximized);
        UiStateRestorer.Apply(form, controls, state, new AppConfig());
        Assert.Equal(FormWindowState.Maximized, form.WindowState);
        Assert.Equal(FormStartPosition.Manual, form.StartPosition);
    }

    private static UiState CreateState() => new()
    {
        RemoteHost = "host",
        MaxPacketSize = 256,
        Attempts = 10,
        LossDelayMs = 500,
        OutputFontSize = 11.5f,
        DynamicPacketSize = false,
        DontFragment = true,
        MonitoringMode = true,
        RipeLookup = false,
        DarkTheme = false,
        IpFamily = 2,
        WindowState = nameof(FormWindowState.Normal),
        Bounds = new UiWindowBounds { X = 100, Y = 100, X2 = 900, Y2 = 700 },
        StatsHeightPercent = 60,
        ColumnWidths = new Dictionary<string, int> { [Strings.Ui.ColumnKeyIp] = 222 }
    };

    private static Form CreateForm() => new()
    {
        StartPosition = FormStartPosition.Manual,
        Bounds = new Rectangle(120, 140, 900, 640)
    };

    private static void FillControls(MainControls controls)
    {
        controls.RemoteHost.Text = " example.com ";
        controls.MaxPacketSize.Value = 512;
        controls.Attempts.Value = 25;
        controls.LossDelay.Value = 300;
        controls.FontSize.Value = 13.5m;
        controls.DynamicPacketSize.Checked = false;
        controls.DontFragment.Checked = true;
        controls.MonitoringMode.Checked = true;
        controls.RipeLookup.Checked = false;
        controls.DarkTheme.Checked = false;
        controls.IpFamily.SelectedIndex = 2;
    }

    private static void SetColumnWidth(DataGridView grid, string key, int width) =>
        grid.Columns[key]!.Width = width;

    private static void AssertCapturedInputs(UiState state)
    {
        Assert.Equal("example.com", state.RemoteHost);
        Assert.Equal(512, state.MaxPacketSize);
        Assert.Equal(25, state.Attempts);
        Assert.Equal(300, state.LossDelayMs);
        Assert.Equal(13.5f, state.OutputFontSize);
        Assert.False(state.DynamicPacketSize);
        Assert.True(state.DontFragment);
        Assert.True(state.MonitoringMode);
        Assert.False(state.RipeLookup);
        Assert.False(state.DarkTheme);
        Assert.Equal(2, state.IpFamily);
        Assert.Equal(nameof(FormWindowState.Normal), state.WindowState);
    }

    private static void AssertCapturedBounds(UiWindowBounds bounds)
    {
        Assert.Equal(120, bounds.X);
        Assert.Equal(140, bounds.Y);
        Assert.Equal(1020, bounds.X2);
        Assert.Equal(780, bounds.Y2);
    }

    private static void AssertRestoredInputs(MainControls controls)
    {
        Assert.Equal("host", controls.RemoteHost.Text);
        Assert.Equal(256m, controls.MaxPacketSize.Value);
        Assert.Equal(10m, controls.Attempts.Value);
        Assert.Equal(500m, controls.LossDelay.Value);
        Assert.Equal(11.5m, controls.FontSize.Value);
        Assert.False(controls.DynamicPacketSize.Checked);
        Assert.True(controls.DontFragment.Checked);
        Assert.True(controls.MonitoringMode.Checked);
        Assert.False(controls.RipeLookup.Checked);
        Assert.False(controls.DarkTheme.Checked);
        Assert.Equal(2, controls.IpFamily.SelectedIndex);
    }

    private static string InvalidStateJson() =>
        """
        {"remoteHost":"host","maxPacketSize":1024,"attempts":0,"lossDelayMs":1000,
         "outputFontSize":9.5,"dynamicPacketSize":true,"darkTheme":true,
         "windowState":"Normal","bounds":{"x":100,"y":100,"x2":900,"y2":700},
         "columnWidths":{"Ip":120}}
        """;
}
