using MvvmScarletToolkit.Commands;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    [MarkupExtensionReturnType(typeof(ICommand))]
    public class StartProcessExtension : MarkupExtension
    {
        public ICommand Command { get; private set; }

        [ConstructorArgument("url")]
        public string Url { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Command is null)
            {
                Command = new RelayCommand(ScarletCommandManager.Default, StartProcess);
            }

            return Command;
        }

        private void StartProcess()
        {
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
