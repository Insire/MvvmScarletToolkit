namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    internal static class TestTimeouts
    {
        /// <summary>
        /// Only ends a test that hangs, it is not a time limit for the tested code. Waits finish as soon as their condition is met.
        /// Build agents run several test processes at the same time, a single wait took 18 seconds there,
        /// so this has to be far above anything a busy machine needs.
        /// </summary>
        public static readonly TimeSpan Hang = TimeSpan.FromMinutes(1);
    }
}
