using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MvvmScarletToolkit.Avalonia.Samples.Features;
using MvvmScarletToolkit.Avalonia.Samples.Views;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Avalonia.Samples;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var navigation = new NavigationViewModel(
                ScarletCommandBuilder.Default,
                new LocalizationsViewModel(new ScarletLocalizationProvider()),
                SynchronizationContext.Current!);

            desktop.MainWindow = new MainWindow { DataContext = navigation, };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
