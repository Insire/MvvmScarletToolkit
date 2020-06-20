using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public class FileSystemBrowser : Control
    {
        private static bool FilterAllFiles(object obj)
        {
            return obj is IFileSystemFile;
        }

        private bool FilterByName(object obj)
        {
            if (!(obj is IFileSystemInfo info))
            {
                return false;
            }

            if (info.IsBusy)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }

            return info.Name.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        /// <summary>Identifies the <see cref="Filter"/> dependency property.</summary>
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(FileSystemBrowser),
            new PropertyMetadata(string.Empty));

        public Predicate<object> FilterAction
        {
            get { return (Predicate<object>)GetValue(FilterActionProperty); }
            set { SetValue(FilterActionProperty, value); }
        }

        /// <summary>Identifies the <see cref="FilterAction"/> dependency property.</summary>
        public static readonly DependencyProperty FilterActionProperty = DependencyProperty.Register(
            nameof(FilterAction),
            typeof(Predicate<object>),
            typeof(FileSystemBrowser),
            new PropertyMetadata(default(Predicate<object>)));

        public Predicate<object> NoFilesFilter
        {
            get { return (Predicate<object>)GetValue(NoFilesFilterProperty); }
            set { SetValue(NoFilesFilterProperty, value); }
        }

        /// <summary>Identifies the <see cref="NoFilesFilter"/> dependency property.</summary>
        public static readonly DependencyProperty NoFilesFilterProperty = DependencyProperty.Register(
            nameof(NoFilesFilter),
            typeof(Predicate<object>),
            typeof(FileSystemBrowser),
            new PropertyMetadata(default(Predicate<object>)));

        public IFileSystemInfo SelectedItem
        {
            get { return (IFileSystemInfo)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>Identifies the <see cref="SelectedItem"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(IFileSystemInfo),
            typeof(FileSystemBrowser),
            new PropertyMetadata(default(IFileSystemInfo)));

        public FileSystemViewModel FileSystemViewModel
        {
            get { return (FileSystemViewModel)GetValue(FileSystemViewModelProperty); }
            set { SetValue(FileSystemViewModelProperty, value); }
        }

        /// <summary>Identifies the <see cref="FileSystemViewModel"/> dependency property.</summary>
        public static readonly DependencyProperty FileSystemViewModelProperty = DependencyProperty.Register(
            nameof(FileSystemViewModel),
            typeof(FileSystemViewModel),
            typeof(FileSystemBrowser),
            new PropertyMetadata(default(FileSystemViewModel)));

        public bool DisplayListView
        {
            get { return (bool)GetValue(DisplayListViewProperty); }
            set { SetValue(DisplayListViewProperty, value); }
        }

        /// <summary>Identifies the <see cref="DisplayListView"/> dependency property.</summary>
        public static readonly DependencyProperty DisplayListViewProperty = DependencyProperty.Register(
            nameof(DisplayListView),
            typeof(bool),
            typeof(FileSystemBrowser),
            new PropertyMetadata(false));

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand DisplayDetailsAsListCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand DisplayDetailsAsIconsCommand { get; }

        public FileSystemBrowser()
        {
            DisplayDetailsAsListCommand = new RelayCommand(ScarletCommandBuilder.Default, ToggleAsListViewInternal);
            DisplayDetailsAsIconsCommand = new RelayCommand(ScarletCommandBuilder.Default, ToggleAsIconsInternal);

            SetCurrentValue(NoFilesFilterProperty, new Predicate<object>(FilterAllFiles));
            SetCurrentValue(FilterActionProperty, new Predicate<object>(FilterByName));
        }

        private void ToggleAsListViewInternal()
        {
            SetCurrentValue(DisplayListViewProperty, true);
        }

        private void ToggleAsIconsInternal()
        {
            SetCurrentValue(DisplayListViewProperty, false);
        }
    }
}
