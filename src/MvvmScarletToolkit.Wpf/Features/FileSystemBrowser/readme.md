# MvvmScarletToolkit.Wpf.FileSystemBrowser

## Goals

This library aims to provide an easy to use and extend browser control for FileSystemAccess.

## Default template

``FileSystemView`` displays:

- a tree of drives and folders.
- the content of the selected tree entry, as a virtualized ``GridView`` or a virtualized list with thumbnails. Switch with ``FileSystemViewModel.ViewMode``.
- a search box. It searches the selected tree entry and its subfolders through ``FileSystemViewModel.SearchText``, supports the wildcards ``*`` and ``?`` and stops after ``FileSystemViewModel.MaxSearchResults`` matches. A query with a wildcard has to match the whole name, any other query matches part of the name.
- an image preview, toggled by ``FileSystemViewModel.IsPreviewVisible``.
- an inspector for size, timestamps, file attributes and permissions of the selected entry, toggled by ``FileSystemViewModel.IsInspectorVisible``. The preview moves from the inspector to the list when the inspector is hidden.

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
