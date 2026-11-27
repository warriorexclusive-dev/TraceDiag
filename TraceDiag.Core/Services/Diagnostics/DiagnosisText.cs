using System;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Formats one or several canonical diagnoses for display.</summary>
internal static class DiagnosisText
{
    private static readonly string[] Separator = [Strings.Diagnosis.Separator];

    internal static string Localize(string canonical)
    {
        var parts = canonical.Split(Separator, StringSplitOptions.None);
        for (var i = 0; i < parts.Length; i++) parts[i] = Strings.T(parts[i]);
        return string.Join(Strings.Diagnosis.Separator, parts);
    }

    internal static bool Contains(string canonical, string reason) =>
        Array.IndexOf(canonical.Split(Separator, StringSplitOptions.None), reason) >= 0;
}
