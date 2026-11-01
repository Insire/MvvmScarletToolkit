using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class ProcessView : UserControl
    {
        // Avalonia has no CompositeCollection
        private readonly ObservableCollection<object> _allOutput = new();
        private ProcessViewModel? _viewModel;

        public ProcessView()
        {
            InitializeComponent();

            AllOutput.ItemsSource = _allOutput;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (_viewModel is not null)
            {
                ((INotifyCollectionChanged)_viewModel.Output).CollectionChanged -= OnOutputChanged;
                ((INotifyCollectionChanged)_viewModel.Errors).CollectionChanged -= OnOutputChanged;
            }

            _viewModel = DataContext as ProcessViewModel;
            _allOutput.Clear();

            if (_viewModel is not null)
            {
                ((INotifyCollectionChanged)_viewModel.Output).CollectionChanged += OnOutputChanged;
                ((INotifyCollectionChanged)_viewModel.Errors).CollectionChanged += OnOutputChanged;

                OnOutputChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void OnOutputChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems!)
                {
                    _allOutput.Add(item);
                }

                return;
            }

            // only happens when a process is started, both lists get cleared then
            _allOutput.Clear();

            if (_viewModel is not null)
            {
                foreach (var item in _viewModel.Output.Cast<object>().Concat(_viewModel.Errors))
                {
                    _allOutput.Add(item);
                }
            }
        }

        private async void SelectProcessFile_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel is null || TopLevel.GetTopLevel(this) is not { } topLevel)
            {
                return;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter =
                [
                    new FilePickerFileType("Executables") { Patterns = ["*.exe", "*.cmd", "*.bat"] },
                    FilePickerFileTypes.All,
                ],
            });

            if (files.Count > 0 && files[0].TryGetLocalPath() is { } path)
            {
                _viewModel.FilePath = path;
            }
        }
    }
}
