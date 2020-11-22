using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
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
                using (Process.Start(Url))
                {
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
