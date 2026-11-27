#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class RequiredMemberAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
{
    internal const string RequiredMembers = "RequiredMembers";

    public string FeatureName { get; } = featureName;

    public bool IsOptional { get; init; }
}
#endif
