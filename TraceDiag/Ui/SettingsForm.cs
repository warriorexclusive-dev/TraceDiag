using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Ui;

/// <summary>Modal window that hosts every control moved off the main toolbar.</summary>
internal sealed class SettingsForm : Form
{
    private readonly MainControls _controls;

    internal SettingsForm(MainControls controls)
    {
        _controls = controls;
        Text = Strings.T(Strings.Ui.SettingsTitle);
        Tag = new UiTextKey(Strings.Ui.SettingsTitle, null);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(720, 520);
        Size = new Size(760, 560);
        Controls.Add(SettingsPageFactory.Create(controls));
        Controls.Add(CreateClosePanel());
        _controls.EditDns.Click += EditDnsOnClick;
    }

    internal void ApplyConfig(AppConfig config)
    {
        var path = string.IsNullOrWhiteSpace(config.AltDnsFilePath)
            ? DnsPathPolicy.Default() : config.AltDnsFilePath;
        _controls.DnsFilePath.Text = path;
        var size = config.IconSizePx.ToString();
        if (_controls.IconSize.Items.Contains(size)) _controls.IconSize.SelectedItem = size;
    }

    internal void UpdateConfig(AppConfig config)
    {
        config.AltDnsFilePath = _controls.DnsFilePath.Text.Trim();
        if (int.TryParse(_controls.IconSize.SelectedItem?.ToString(), out var size)) config.IconSizePx = size;
    }

    private Panel CreateClosePanel()
    {
        var close = CreateButton(Strings.Ui.SaveButton);
        close.DialogResult = DialogResult.OK;
        _controls.AdapterNote.Location = new Point(12, 16);
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 48 };
        panel.Controls.Add(_controls.AdapterNote);
        panel.Controls.Add(close);
        return panel;
    }

    private static Button CreateButton(string canonical)
    {
        var button = new Button { DialogResult = DialogResult.OK, Size = new Size(120, 32),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(620, 8) };
        UiText.Set(button, canonical);
        return button;
    }

    private void EditDnsOnClick(object? sender, EventArgs e)
    {
        using var editor = new AltDnsEditorForm(_controls.DnsFilePath.Text.Trim());
        if (editor.ShowDialog(this) == DialogResult.OK) _controls.DnsFilePath.Text = editor.Path;
    }
}
