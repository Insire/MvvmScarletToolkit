# MvvmScarletToolkit

Viewmodel-first MVVM toolkit for WPF and Avalonia. See [README.md](README.md) for the feature overview and each `src/*/readme.md` for package details.

## Commands

```bash
dotnet build MvvmScarletToolkit.slnx
dotnet test --solution MvvmScarletToolkit.slnx
dotnet run --project samples/MvvmScarletToolkit.Wpf.Samples
dotnet run --project samples/MvvmScarletToolkit.Avalonia.Samples
```

`build.ps1` / `build.sh` run the Cake pipeline in `cake/`. Both also format XAML in `src` with XamlStyler (`Settings.XamlStyler`). CI fails for unformatted XAML in `src`, so after changing XAML there run `dotnet xstyler -d src -r -c Settings.XamlStyler` (after `dotnet tool restore`).

Tests use xunit v3 on Microsoft.Testing.Platform (see `global.json`). WPF projects, tests and samples only run on Windows.

## Layout

- `src/MvvmScarletToolkit.Abstractions`, `MvvmScarletToolkit`, `.Commands`, `.Observables`: UI-independent, targeting `netstandard2.0` and .NET 8. Code has to compile for `netstandard2.0`, so newer BCL APIs are not available there. The .NET 8 target exists for the trim and AOT analyzers.
- All non-WPF packages import `aot-compatible.props`, which enables `IsAotCompatible` and links the trim attribute polyfills from `src/Shared/TrimAnnotations` into the `netstandard2.0` build. They must build without IL warnings.
- `src/MvvmScarletToolkit.ImageLoading`, `.Mediator`: UI-independent, targeting .NET 8 and 10.
- `src/MvvmScarletToolkit.Wpf`, `.Avalonia`: UI-specific packages, targeting .NET 8 and 10.
- `src/Shared/HtmlTextBlock`: internal html parser, compiled into both UI packages as linked files. No WPF or Avalonia types there.
- `samples/`: working demos. `MvvmScarletToolkit.Wpf.Samples/MainWindow.xaml` holds the scene templates, `Features/NavigationViewModel.cs` registers the scenes.
- `tests/`: one test project per package, plus `shared/MvvmScarletToolkit.Test.Tracing`. `MvvmScarletToolkit.Aot.Tests` has no tests of its own: it compiles the test files of the AOT compatible packages as linked files and runs them as a Native AOT executable. It leaves out files that use NSubstitute, and test files compiled into it have to build with `XUNIT_AOT` (no `IXunitSerializable`, no generic test methods, no `Assert.Equivalent`).
- Package versions are managed centrally in `Directory.Packages.props`.
- `ci.yaml` (dev, pre-release packages) and `cd.yaml` (master, nuget.org) are the Azure Pipelines. Their Test, Pack, Native AOT and XAML format jobs are shared templates in `pipelines/`, only the Publish jobs differ. Change a job in its template, not in one of the pipelines.

## Conventions

- Block-scoped namespaces, braces on new lines, `var` where the type is apparent. `.editorconfig` is authoritative.
- WPF code: behaviors live in `Behaviors/` under namespace `MvvmScarletToolkit.Wpf`, features and attached properties use `MvvmScarletToolkit`. New namespaces must be added as an `XmlnsDefinition` in `src/MvvmScarletToolkit.Wpf/Properties/AssemblyInfo.cs` to be reachable through the `mvvm:` prefix.
- Behaviors derive from `Microsoft.Xaml.Behaviors.Behavior<T>` and document their required xmlns in an XML doc `<remarks>` block.
- Viewmodels derive from `ViewModelBase`, `ViewModelListBase<T>` or `BusinessViewModelListBase<T>` and build commands through `IScarletCommandBuilder`. UI work goes through `Dispatcher.Invoke`.
- `Messenger` is shared by all viewmodels from the same command builder. Handlers must filter by sender or value, otherwise they react to unrelated instances.
- Tests that touch WPF objects run through `StaRunner.Run` (`tests/MvvmScarletToolkit.Wpf.Tests/Util`) and derive from `TraceTestBase`. `StaRunner` moves them to the hidden desktop of `TestDesktopFixture`, so windows have to be created on its thread to stay off the user's screen.
- Tests must not assert absolute `TimeSpan` or `DateTime` values, like a fixed time limit or the current time. Make them relative instead, e.g. compare the duration of two input sizes (see `tests/shared/HtmlParserScaling.cs`) or one point in time with another. Scrub timestamps and durations from snapshots.
- When public behavior changes, update the matching package `readme.md` and add or extend a sample.

## Git

- `dev` is the working branch, PRs go to `master`.
- Commit messages are short imperative sentences, e.g. `Fix notification dispatch for typed handlers`.
