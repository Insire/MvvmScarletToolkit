namespace MvvmScarletToolkit.ImageLoading.Tests;

/// <summary>
/// Records exceptions thrown (even if caught) within an async flow. Tests run in parallel, so only the flow started by
/// <see cref="CaptureAsync"/> is recorded. Thrown exceptions are expensive, especially with a debugger attached.
/// </summary>
internal static class FirstChanceExceptions
{
    private static readonly AsyncLocal<List<Exception>?> _current = new();

    static FirstChanceExceptions()
    {
        AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
        {
            var list = _current.Value;
            if (list is null)
            {
                return;
            }

            lock (list)
            {
                list.Add(e.Exception);
            }
        };
    }

    public static async Task<IReadOnlyList<Exception>> CaptureAsync(Func<Task> action)
    {
        var list = new List<Exception>();
        _current.Value = list;

        try
        {
            await action();
        }
        finally
        {
            _current.Value = null;
        }

        lock (list)
        {
            return list.ToArray();
        }
    }
}
