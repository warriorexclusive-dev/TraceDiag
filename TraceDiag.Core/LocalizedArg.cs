namespace TraceDiag;

/// <summary>Carries one canonical English value and its display translation.</summary>
internal sealed class LocalizedArg
{
    private LocalizedArg(string canonical, string localized)
    {
        Canonical = canonical;
        Localized = localized;
    }

    internal string Canonical { get; }

    internal string Localized { get; }

    public override string ToString() => Canonical;

    internal static LocalizedArg Create(string canonical, string localized) => new(canonical, localized);

    internal static LocalizedArg Of(string text) => Create(text, Strings.T(text));

    internal static LocalizedArg Format(string canonicalFormat, params object?[] args) =>
        Create(TextFormatter.Fmt(canonicalFormat, args), TextFormatter.Fmt(Strings.T(canonicalFormat), args));
}
