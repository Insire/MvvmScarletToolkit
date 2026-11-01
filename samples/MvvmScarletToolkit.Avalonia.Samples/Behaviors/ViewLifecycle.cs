using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MvvmScarletToolkit.Avalonia.Samples.Behaviors
{
    /// <summary>
    /// Executes commands when a control gets loaded and unloaded, what EventTrigger and InvokeCommandAction do in the WPF sample.
    /// Don't use this inside of item templates, containers get recycled there. See <see cref="ContainerLifecycle{TItem}"/> for that case.
    /// </summary>
    public sealed class ViewLifecycle
    {
        // the DataContext is already gone when Unloaded is raised, so the last command is remembered
        private static readonly ConditionalWeakTable<Control, ICommand> _unloadCommands = new();

        public static readonly AttachedProperty<ICommand?> LoadCommandProperty = AvaloniaProperty.RegisterAttached<ViewLifecycle, Control, ICommand?>("LoadCommand");

        public static readonly AttachedProperty<ICommand?> UnloadCommandProperty = AvaloniaProperty.RegisterAttached<ViewLifecycle, Control, ICommand?>("UnloadCommand");

        static ViewLifecycle()
        {
            LoadCommandProperty.Changed.AddClassHandler<Control, ICommand?>(OnLoadCommandChanged);
            UnloadCommandProperty.Changed.AddClassHandler<Control, ICommand?>(OnUnloadCommandChanged);
        }

        private ViewLifecycle()
        {
        }

        public static ICommand? GetLoadCommand(Control control) => control.GetValue(LoadCommandProperty);

        public static void SetLoadCommand(Control control, ICommand? value) => control.SetValue(LoadCommandProperty, value);

        public static ICommand? GetUnloadCommand(Control control) => control.GetValue(UnloadCommandProperty);

        public static void SetUnloadCommand(Control control, ICommand? value) => control.SetValue(UnloadCommandProperty, value);

        private static void OnLoadCommandChanged(Control control, AvaloniaPropertyChangedEventArgs<ICommand?> e)
        {
            control.Loaded -= OnLoaded;
            control.Loaded += OnLoaded;
        }

        private static void OnUnloadCommandChanged(Control control, AvaloniaPropertyChangedEventArgs<ICommand?> e)
        {
            if (e.NewValue.GetValueOrDefault() is { } command)
            {
                _unloadCommands.AddOrUpdate(control, command);
                control.Unloaded -= OnUnloaded;
                control.Unloaded += OnUnloaded;
            }
        }

        private static void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                Execute(GetLoadCommand(control));
            }
        }

        private static void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control control && _unloadCommands.TryGetValue(control, out var command))
            {
                Execute(command);
            }
        }

        private static void Execute(ICommand? command)
        {
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }
    }
}
