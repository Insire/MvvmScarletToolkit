using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Commands
{
    public interface ICommandBuilderContext : IBuilder<IExtendedAsyncCommand>
    {
        IScarletDispatcher Dispatcher { get; }
        IScarletCommandManager CommandManager { get; }

        ICancelCommand CancelCommand { get; set; }

        void AddDecorator(Func<IExtendedAsyncCommand, IExtendedAsyncCommand> decoratorFactory);
    }
}
