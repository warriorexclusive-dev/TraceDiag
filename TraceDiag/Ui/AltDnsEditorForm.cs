using System.Net;
using TraceDiag.Services;

namespace TraceDiag.Ui;

/// <summary>Edits the alternative DNS list with validity and live resolve checks.</summary>
internal sealed class AltDnsEditorForm : Form
{
    private readonly List<IPAddress> _addresses = [];
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };
    private readonly TextBox _input = new() { Dock = DockStyle.Top };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, AutoSize = true };
    private readonly Label _hint = new() { Dock = DockStyle.Bottom, AutoSize = true };
    private readonly Button _add;
    private readonly Button _remove;
    private readonly Button _save;
    private readonly Button _cancel;

    internal AltDnsEditorForm(string path)
    {
        Path = string.IsNullOrWhiteSpace(path) ? DnsPathPolicy.Default() : path;
        Text = Strings.T(Strings.Ui.DnsListTitle);
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(480, 420);
        _add = CreateButton(Strings.Ui.AddButton);
        _remove = CreateButton(Strings.Ui.RemoveButton);
        _save = CreateButton(Strings.Ui.SaveButton);
        _cancel = CreateButton(Strings.Ui.CancelButton);
        _save.DialogResult = DialogResult.OK;
        _cancel.DialogResult = DialogResult.Cancel;
        _add.Click += AddOnClick;
        _remove.Click += RemoveOnClick;
        Controls.Add(_list);
        Controls.Add(CreateActionsPanel());
        Controls.Add(_status);
        Controls.Add(_input);
        UiText.Set(_hint, Strings.Ui.DnsListHint);
        Controls.Add(_hint);
        foreach (var server in DnsServerConfig.Read(Path)) _addresses.Add(server);
        RefreshList();
    }

    internal string Path { get; }

    private Panel CreateActionsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 48 };
        _add.Location = new Point(8, 8);
        _remove.Location = new Point(144, 8);
        _save.Location = new Point(280, 8);
        _cancel.Location = new Point(416, 8);
        panel.Controls.Add(_add);
        panel.Controls.Add(_remove);
        panel.Controls.Add(_save);
        panel.Controls.Add(_cancel);
        return panel;
    }

    private static Button CreateButton(string canonical)
    {
        var button = new Button { Size = new Size(120, 32) };
        UiText.Set(button, canonical);
        return button;
    }

    private async void AddOnClick(object? sender, EventArgs e)
    {
        if (!IPAddress.TryParse(_input.Text.Trim(), out var address))
        {
            SetStatus(TextFormatter.Fmt(Strings.Ui.InvalidIpFormat, _input.Text));
            return;
        }
        _add.Enabled = false;
        var responds = await DnsServerProbe.RespondsAsync(address, CancellationToken.None);
        _add.Enabled = true;
        SetStatus(TextFormatter.Fmt(responds ? Strings.Ui.ResolveOkFormat
            : Strings.Ui.ResolveFailedFormat, address));
        if (!responds) return;
        if (!_addresses.Contains(address)) _addresses.Add(address);
        RefreshList();
    }

    private void RemoveOnClick(object? sender, EventArgs e)
    {
        if (_list.SelectedIndex < 0) return;
        _addresses.RemoveAt(_list.SelectedIndex);
        RefreshList();
    }

    private void RefreshList()
    {
        _list.DataSource = null;
        _list.DataSource = _addresses.Select(address => address.ToString()).ToArray();
    }

    private void SetStatus(string text) => _status.Text = text;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            try
            {
                DnsServerConfig.Save(Path, _addresses);
                SetStatus(TextFormatter.Fmt(Strings.Ui.DnsSavedFormat, _addresses.Count, Path));
            }
            catch (Exception ex)
            {
                SetStatus(TextFormatter.Fmt(Strings.Ui.DnsSaveFailedFormat, ex.Message));
                e.Cancel = true;
            }
        }
        base.OnFormClosing(e);
    }
}
