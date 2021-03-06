using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    // xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"
    // usage: <Hyperlink Command="{mvvm:StartProcess Url='www.github.com/Insire/Dawn'}"/>
    [MarkupExtensionReturnType(typeof(ICommand))]
    public sealed class StartProcessExtension : MarkupExtension
    {
        public ICommand? Command { get; private set; }

        [ConstructorArgument("url")]
        public string? Url { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Command is null)
            {
                Command = new RelayCommand(StartProcess);
            }

            return Command;
        }

        private void StartProcess()
        {
            if (Url is null || Url.Length == 0)
            {
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (Process.Start(new ProcessStartInfo("cmd", $"/c start {Url}")))
                    { }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    using (Process.Start("xdg-open", Url))
                    { }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    using (Process.Start("open", Url))
                    { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
