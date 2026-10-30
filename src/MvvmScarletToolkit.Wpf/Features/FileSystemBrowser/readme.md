# MvvmScarletToolkit.Wpf.FileSystemBrowser

## Goals

This library aims to provide an easy to use and extend browser control for FileSystemAccess.

## Contents

|class|summary|
|---|---|
|[``FileSystemView``](FileSystemView.cs)|Control that displays a ``FileSystemViewModel``, styled by ``Generic.xaml``.|
|[``FileSystemViewModel``](FileSystemViewModel.cs)|Provides the drives, the selected folder and its content, and the selected file.|
|[``FileSystemOptionsViewModel``](FileSystemOptionsViewModel.cs)|Selects which drive types, file attributes and folder attributes are shown. ``Default`` provides a preset.|
|[``FilePickerViewModel``](FilePickerViewModel.cs)|Wraps a ``FileSystemViewModel`` for selecting a file.|
|[``FolderPickerViewModel``](FolderPickerViewModel.cs)|Wraps a ``FileSystemViewModel`` for selecting a folder.|
|[``FileSystemViewModelFactory``](FileSystemViewModelFactory.cs)|Creates the viewmodels for drives, folders and files.|
|[``Generic.xaml``](Generic.xaml)|Default styles. Merge this into your application resources, as described in the [MvvmScarletToolkit.Wpf](../../readme.md) readme.|

## Licensing

The FileSystemBrowser uses some icons from the [Font Awesome](https://fontawesome.com/) icon set.

You can find the License [here](https://fontawesome.com/license).

The icons being used are:

- [File icon](https://fontawesome.com/icons/file?style=regular)
- [Folder icon](https://fontawesome.com/icons/folder?style=regular)
- [Drive icon](https://fontawesome.com/icons/hdd?style=regular)
