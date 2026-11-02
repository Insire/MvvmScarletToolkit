namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed record ScarletAccessRule(string Identity, string Rights, bool IsDenied, bool IsInherited);

    public sealed record ScarletPermissions(string? Owner, IReadOnlyList<ScarletAccessRule> Rules);
}
