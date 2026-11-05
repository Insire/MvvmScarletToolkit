---
name: new-behavior
description: Scaffold a new WPF behavior in MvvmScarletToolkit.Wpf with docs, xmlns registration, a sample scene and STA tests.
argument-hint: <BehaviorName> <AssociatedType> [what it should do]
disable-model-invocation: true
---

# New WPF behavior

Create the behavior described in `$ARGUMENTS` (name, associated element type, purpose). If the name or the associated type is missing, ask before writing anything.

Work through every step. Report which ones you completed at the end.

## 1. Behavior class

- File: `src/MvvmScarletToolkit.Wpf/Behaviors/<Name>.cs`. The name ends in `Behavior`.
- Namespace `MvvmScarletToolkit.Wpf`, block-scoped, braces on new lines, `var` where the type is apparent.
- `public sealed class <Name> : Behavior<TAssociated>` from `Microsoft.Xaml.Behaviors`.
- Subscribe in `OnAttached`, unsubscribe in `OnDetaching`, and call `base` in both. If the behavior hooks visual-tree children (ScrollViewer, Popup), also handle `Loaded`/`Unloaded` the way `AutoScrollBehavior` does.
- Configurable options are `DependencyProperty` fields registered with `nameof`.
- XML docs: a `<summary>`, plus a `<remarks>` block listing the required namespaces in exactly this format:

```csharp
/// <remarks>
/// required namespaces:
/// <list type="bullet">
/// <item>
/// <description>xmlns:i="http://schemas.microsoft.com/xaml/behaviors"</description>
/// </item>
/// <item>
/// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
/// </item>
/// </list>
/// </remarks>
// usage:
// <i:Interaction.Behaviors>
//    <mvvm:<Name> />
// </ i:Interaction.Behaviors>
```

Use `AutoScrollBehavior.cs` as the reference implementation.

## 2. xmlns registration

Behaviors in `MvvmScarletToolkit.Wpf` are already reachable through `mvvm:`. If you introduced any **new** namespace (helper types, enums), add an `XmlnsDefinition` for it in `src/MvvmScarletToolkit.Wpf/Properties/AssemblyInfo.cs`, next to the existing entries.

## 3. Tests

- File: `tests/MvvmScarletToolkit.Wpf.Tests/<Name>Tests.cs`, namespace `MvvmScarletToolkit.Wpf.Tests`.
- `public sealed class <Name>Tests : TraceTestBase`.
- Every test that touches WPF objects returns `StaRunner.Run(async () => { ... })`. Create all controls and windows inside that lambda so they land on the hidden test desktop.
- Show elements with `Ui.Show(element)`, wait with `await Ui.Settle()`, and close the window at the end. Look at `ScrollingBehaviorTests.cs` and `tests/MvvmScarletToolkit.Wpf.Tests/Util/` for the helpers.
- Cover attach/detach (no leaked handlers after `Detach`), the main behavior, and each dependency property.

## 4. Sample

- Add a viewmodel under `samples/MvvmScarletToolkit.Wpf.Samples/Features/` (derive from `ViewModelBase` and take `IScarletCommandBuilder` if it needs commands).
- Register it in `Features/NavigationViewModel.cs` with `Add("<Readable title>", new <Vm>(...))`.
- Add a `DataTemplate` for the viewmodel in `MainWindow.xaml` that uses the behavior.

## 5. Docs

Add the behavior to `src/MvvmScarletToolkit.Wpf/readme.md` in the matching section, with a short description and a XAML usage snippet.

## 6. Verify

```bash
dotnet build MvvmScarletToolkit.slnx
dotnet test --project tests/MvvmScarletToolkit.Wpf.Tests/MvvmScarletToolkit.Wpf.Tests.csproj
```

Fix every warning and failure you introduced before reporting back.
