# MvvmScarletToolkit

Viewmodel-first MVVM toolkit for WPF and Avalonia. See [README.md](README.md) for the feature overview and each `src/*/readme.md` for package details.

## Commands

```bash
dotnet build MvvmScarletToolkit.slnx
dotnet test --solution MvvmScarletToolkit.slnx
dotnet run --project samples/MvvmScarletToolkit.Wpf.Samples
dotnet run --project samples/MvvmScarletToolkit.Avalonia.Samples
```

`build.ps1` / `build.sh` run the Cake pipeline in `cake/`. `build.ps1` also formats XAML in `src` with XamlStyler (`Settings.XamlStyler`).

Tests use xunit v3 on Microsoft.Testing.Platform (see `global.json`). WPF projects, tests and samples only run on Windows.

## Layout

- `src/MvvmScarletToolkit.Abstractions`, `MvvmScarletToolkit`, `.Commands`, `.Observables`: UI-independent and `netstandard2.0`, so newer BCL APIs are not available there.
- `src/MvvmScarletToolkit.ImageLoading`, `.Mediator`: UI-independent, targeting .NET 8 and 10.
- `src/MvvmScarletToolkit.Wpf`, `.Avalonia`: UI-specific packages, targeting .NET 8 and 10.
- `samples/`: working demos. `MvvmScarletToolkit.Wpf.Samples/MainWindow.xaml` holds the scene templates, `Features/NavigationViewModel.cs` registers the scenes.
- `tests/`: one test project per package, plus `shared/MvvmScarletToolkit.Test.Tracing`.
- Package versions are managed centrally in `Directory.Packages.props`.

## Conventions

- Block-scoped namespaces, braces on new lines, `var` where the type is apparent. `.editorconfig` is authoritative.
- WPF code: behaviors live in `Behaviors/` under namespace `MvvmScarletToolkit.Wpf`, features and attached properties use `MvvmScarletToolkit`. New namespaces must be added as an `XmlnsDefinition` in `src/MvvmScarletToolkit.Wpf/Properties/AssemblyInfo.cs` to be reachable through the `mvvm:` prefix.
- Behaviors derive from `Microsoft.Xaml.Behaviors.Behavior<T>` and document their required xmlns in an XML doc `<remarks>` block.
- Viewmodels derive from `ViewModelBase`, `ViewModelListBase<T>` or `BusinessViewModelListBase<T>` and build commands through `IScarletCommandBuilder`. UI work goes through `Dispatcher.Invoke`.
- `Messenger` is shared by all viewmodels from the same command builder. Handlers must filter by sender or value, otherwise they react to unrelated instances.
- Tests that touch WPF objects run through `StaRunner.Run` (`tests/MvvmScarletToolkit.Wpf.Tests/Util`) and derive from `TraceTestBase`.
- When public behavior changes, update the matching package `readme.md` and add or extend a sample.

## Git

- `dev` is the working branch, PRs go to `master`.
- Commit messages are short imperative sentences, e.g. `Fix notification dispatch for typed handlers`.
