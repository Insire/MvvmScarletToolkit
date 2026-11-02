# MvvmScarletToolkit.Wpf.FileSystemBrowser

## Goals

This library aims to provide an easy to use and extend browser control for FileSystemAccess.

## Default template

``FileSystemView`` displays:

- a tree of drives and folders.
- the content of the selected tree entry, as a virtualized ``GridView``, a virtualized list with thumbnails or virtualized cards, where confirmed images show their thumbnail instead of an icon. Switch with ``FileSystemViewModel.ViewMode``.
- a search box. It searches the selected tree entry and its subfolders through ``FileSystemViewModel.SearchText``, supports the wildcards ``*`` and ``?`` and stops after ``FileSystemViewModel.MaxSearchResults`` matches. A query with a wildcard has to match the whole name, any other query matches part of the name.
- an image preview, toggled by ``FileSystemViewModel.IsPreviewVisible``.
- an inspector for size, timestamps, file attributes and permissions of the selected entry, toggled by ``FileSystemViewModel.IsInspectorVisible``. The preview moves from the inspector to the list when the inspector is hidden.

## Dialogs

``FileSystemDialog`` asks for a file or folder in a modal window and returns the path, or ``null`` when the user canceled. It can be called from any thread and cancelling the token closes the window. The default template has to be available, see ``Generic.xaml`` below.

```cs
var dialog = new FileSystemDialog(scheduler, factory, FileSystemOptionsViewModel.Default, commandBuilder.Dispatcher);

var file = await dialog.PickFile(initialFolder: null, token);
var folder = await dialog.PickFolder(@"C:\Temp", token);
```

Both start in the given folder, or the deepest part of it that exists, and in the documents folder without one. The file dialog confirms with a double click on a file. The folder dialog lists folders only and returns the selected folder, or the displayed one if none is selected. For a custom window, bind ``FilePickerViewModel`` or ``FolderPickerViewModel`` and handle their ``Confirmed`` event.

``PickFile`` optionally takes the files that can be selected. The first filter is applied initially, the user can switch between them in the window. With a single filter the dropdown only displays it and is disabled. Add ``FileSystemFilter.AllFiles`` to allow every file, without it only the given filters can be used.

```cs
var images = new FileSystemFilter("Images", "*.png", "*.jpg");
var text = new FileSystemFilter("Text files", "*.txt");

var file = await dialog.PickFile(initialFolder: null, token, images, text);
```

Filters match the file name, not the content. They work like the search, so ``*.png`` has to match the whole name and a pattern without a wildcard matches part of it. ``FileSystemViewModel.FileFilter`` applies a filter to any browser, in the detail view and the search.

All ``FileSystemViewModel``s of a factory share its messenger. They ignore selections from each other, so a dialog doesn't change the selection of another browser. ``Dispose`` stops listening.

## File types

The file extension is only an expectation. ``ScarletFile.MimeType`` starts out with the type of the extension, without opening the file. ``ScarletFile.CheckHeaderCommand`` reads the file header, which overrides the extension. ``ScarletFile.IsImage`` is only ``true`` once the header confirmed an image. The default template runs this command for displayed and selected files, custom templates need to do the same to get previews.

## Contents

|class|summary|
|---|---|
|[``FileSystemView``](FileSystemView.cs)|Control that displays a ``FileSystemViewModel``, styled by ``Generic.xaml``.|
|[``FileSystemViewModel``](FileSystemViewModel.cs)|Provides the drives, the selected folder and its content, the selected entry, the search and the state of the preview and inspector.|
|[``FileSystemSearch``](FileSystemSearch.cs)|Matches names against a search query.|
|[``MimeTypeNames``](MimeTypeNames.cs)|Maps mime types to displayed names.|
|[``FileSystemOptionsViewModel``](FileSystemOptionsViewModel.cs)|Selects which drive types, file attributes and folder attributes are shown. ``Default`` provides a preset.|
|[``FileSystemDialog``](FileSystemDialog.cs)|Shows a modal window to select a file or folder.|
|[``FileSystemDialogWindow``](FileSystemDialogWindow.xaml)|The window of a ``FileSystemPickerViewModel``, a ``FileSystemView`` with confirm and cancel buttons.|
|[``FileSystemFilter``](FileSystemFilter.cs)|Restricts the displayed files by name patterns.|
|[``FileSystemPickerViewModel``](FileSystemPickerViewModel.cs)|Base of the pickers, provides the ``SelectedPath``, confirming it and the filters.|
|[``FilePickerViewModel``](FilePickerViewModel.cs)|Wraps a ``FileSystemViewModel`` for selecting a file.|
|[``FolderPickerViewModel``](FolderPickerViewModel.cs)|Wraps a ``FileSystemViewModel`` for selecting a folder.|
|[``FileSystemViewModelFactory``](FileSystemViewModelFactory.cs)|Creates the viewmodels for drives, folders and files.|
|[``FileSystemViewModelFactoryExtensions``](FileSystemViewModelFactoryExtensions.cs)|Loads the folders and files of a parent in a single call.|
|[``RegistryFileExtensionMimeTypeResolver``](MimeTypeResolvers/RegistryFileExtensionMimeTypeResolver.cs), [``StaticFileExtensionMimeTypeResolver``](MimeTypeResolvers/StaticFileExtensionMimeTypeResolver.cs), [``MagicNumberMimeTypeResolver``](MimeTypeResolvers/MagicNumberMimeTypeResolver.cs)|The ``IMimeTypeResolver`` implementations used by the factory. They work from the Windows registry, a fixed list of file extensions and the content of a file.|
|[``Generic.xaml``](Generic.xaml)|Default styles. Merge this into your application resources, as described in the [MvvmScarletToolkit.Wpf](../../readme.md) readme.|

## Licensing

The FileSystemBrowser uses some icons from the [Font Awesome](https://fontawesome.com/) icon set.

You can find the License [here](https://fontawesome.com/license).

The icons being used are:

- [File icon](https://fontawesome.com/icons/file?style=regular)
- [Folder icon](https://fontawesome.com/icons/folder?style=regular)
- [Drive icon](https://fontawesome.com/icons/hdd?style=regular)
