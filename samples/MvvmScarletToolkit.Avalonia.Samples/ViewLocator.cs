using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MvvmScarletToolkit.Avalonia.Samples.Features;
using MvvmScarletToolkit.Avalonia.Samples.Features.Geometry;
using MvvmScarletToolkit.Avalonia.Samples.Views;
using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.ContextMenu;
using MvvmScarletToolkit.Core.Samples.Features.Dialog;
using MvvmScarletToolkit.Core.Samples.Features.Enums;
using MvvmScarletToolkit.Core.Samples.Features.Image;
using MvvmScarletToolkit.Core.Samples.Features.NestedSelection;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;

namespace MvvmScarletToolkit.Avalonia.Samples
{
    public sealed class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Type> _viewMap;

        public ViewLocator()
        {
            _viewMap = new Dictionary<Type, Type>()
            {
                [typeof(ProcessingImagesViewModel)] = typeof(DragAndDropView),
                [typeof(DataEntriesViewModel)] = typeof(DataEntriesView),
                [typeof(VirtualizedImagesViewModel)] = typeof(VirtualizedImagesView),
                [typeof(AsyncStateListViewModel)] = typeof(AsyncStateListView),
                [typeof(ProgressViewModel)] = typeof(ProgressView),
                [typeof(BusyViewModel)] = typeof(BusyView),
                [typeof(GeometryRenderViewModel)] = typeof(GeometryRenderView),
                [typeof(PasswordViewModel)] = typeof(PasswordView),
                [typeof(DialogViewModel)] = typeof(DialogView),
                [typeof(ProcessViewModel)] = typeof(ProcessView),
                [typeof(ContextMenuViewModels)] = typeof(ContextMenuView),
                [typeof(EnumsViewModel)] = typeof(EnumsView),
                [typeof(ToastsViewModel)] = typeof(ToastsView),
                [typeof(FormViewModel)] = typeof(FormView),
                [typeof(ObservableDictionaryViewModel)] = typeof(ObservableDictionaryView),
                [typeof(SelectionsViewModel)] = typeof(SelectionsView),
            };
        }

        public Control? Build(object? viewmodel)
        {
            if (viewmodel is null)
            {
                return new TextBlock { Text = "NULL" };
            }

            return (Control)Activator.CreateInstance(_viewMap[viewmodel.GetType()])!;
        }

        public bool Match(object? data)
        {
            // only the scenes, this would otherwise take over every viewmodel that is shown as content
            return data is not null && _viewMap.ContainsKey(data.GetType());
        }
    }
}
