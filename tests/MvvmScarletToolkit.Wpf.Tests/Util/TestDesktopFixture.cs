using System.Runtime.InteropServices;

namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    /// <summary>
    /// A hidden desktop shared by all tests of the assembly. <see cref="StaRunner"/> moves its threads there, so that
    /// windows of tests are not visible and don't take the focus from whoever uses the machine while tests are running.
    /// </summary>
    public sealed class TestDesktopFixture : IDisposable
    {
        private const uint GenericAll = 0x10000000;
        private const uint ApartmentThreaded = 0x2;

        private static nint _desktop;

        /// <summary>
        /// Whether the hidden desktop could be created, tests stay on the current desktop otherwise.
        /// </summary>
        internal static bool IsAvailable => _desktop != 0;

        public TestDesktopFixture()
        {
            // the name is unique per process, the frameworks of the test project run in parallel
            _desktop = CreateDesktop($"{nameof(MvvmScarletToolkit)}.Tests.{Environment.ProcessId}", null, 0, 0, GenericAll, 0);
        }

        /// <summary>
        /// Has to be called before the thread creates its dispatcher, a thread with windows can't change its desktop.
        /// </summary>
        internal static void Enter()
        {
            if (!IsAvailable)
            {
                return;
            }

            // COM creates a window for STA threads before any managed code runs and threads with windows can't change their desktop.
            // Leaving the apartment removes the window, the runtime keeps treating the thread as STA
            CoUninitialize();

            if (!SetThreadDesktop(_desktop))
            {
                var error = Marshal.GetLastPInvokeErrorMessage();
                Marshal.ThrowExceptionForHR(CoInitializeEx(0, ApartmentThreaded));

                throw new InvalidOperationException($"Failed to enter the test desktop: {error}");
            }

            Marshal.ThrowExceptionForHR(CoInitializeEx(0, ApartmentThreaded));
        }

        internal static bool IsCurrentThreadOnDesktop()
        {
            return GetThreadDesktop(GetCurrentThreadId()) == _desktop;
        }

        public void Dispose()
        {
            if (IsAvailable)
            {
                CloseDesktop(_desktop);
                _desktop = 0;
            }
        }

        [DllImport("user32.dll", EntryPoint = "CreateDesktopW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nint CreateDesktop(string name, string? device, nint devMode, uint flags, uint desiredAccess, nint securityAttributes);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetThreadDesktop(nint desktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint GetThreadDesktop(uint threadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseDesktop(nint desktop);

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(nint reserved, uint coInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
