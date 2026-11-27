using TraceDiag.Models;

namespace TraceDiag.Services;

internal sealed record UiStateLoadResult(UiState? State, bool Corrupted);

internal sealed class UiStateStore
{
    internal UiStateStore(string filePath) => FilePath = filePath;

    internal string FilePath { get; }

    internal UiStateLoadResult Load()
    {
        try { return new UiStateLoadResult(UiStateFileIO.Load(FilePath), false); }
        catch { Reset(); return new UiStateLoadResult(null, true); }
    }

    internal bool Save(UiState state)
    {
        try { UiStateFileIO.Save(FilePath, state); return true; }
        catch { return false; }
    }

    internal void Reset()
    {
        try { if (File.Exists(FilePath)) File.Delete(FilePath); }
        catch
        {
            // State reset errors should not break the app.
        }
    }
}
