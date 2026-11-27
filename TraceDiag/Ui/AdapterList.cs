using TraceDiag.Services;

namespace TraceDiag.Ui;

/// <summary>Binds the adapter combo box to identifiers instead of display names.</summary>
internal static class AdapterList
{
    internal static void Fill(ComboBox combo)
    {
        var adapters = NetworkAdapterCatalog.Get();
        combo.Items.Clear();
        combo.Items.Add(Strings.T(Strings.Ui.AutoAdapter));
        foreach (var adapter in adapters) combo.Items.Add(adapter.Name);
        combo.Tag = new[] { string.Empty }.Concat(adapters.Select(a => a.Id)).ToArray();
        combo.SelectedIndex = 0;
    }

    internal static string? SelectedId(ComboBox combo)
    {
        var ids = combo.Tag as string[];
        var index = combo.SelectedIndex;
        return ids is null || index < 0 || index >= ids.Length ? null : ids[index];
    }

    internal static void Select(ComboBox combo, string? id)
    {
        var ids = combo.Tag as string[] ?? [];
        var index = Array.FindIndex(ids, item => item == (id ?? string.Empty));
        combo.SelectedIndex = index < 0 ? 0 : index;
    }
}
