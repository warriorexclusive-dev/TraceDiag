using System.Text.Json;
using TraceDiag.Models;

namespace TraceDiag.Services;

internal static class UiStateFileIO
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    internal static UiState? Load(string path)
    {
        if (!File.Exists(path)) return null;
        var state = JsonSerializer.Deserialize<UiState>(File.ReadAllText(path), Options);
        if (state is null || !UiStateValidator.IsValid(state)) throw new InvalidDataException(nameof(UiState));
        return state;
    }

    internal static void Save(string path, UiState state)
    {
        EnsureDirectory(path);
        File.WriteAllText(path, JsonSerializer.Serialize(state, Options));
    }

    private static void EnsureDirectory(string path)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }
        catch (ArgumentException) { /* Invalid paths must not break diagnostics. */ }
    }
}
