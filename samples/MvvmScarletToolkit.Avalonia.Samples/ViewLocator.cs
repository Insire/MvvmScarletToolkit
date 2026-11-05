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
using MvvmScarletToolkit.Core.Samples.Features.TagCloud;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;

namespace MvvmScarletToolkit.Avalonia.Samples
{
    public sealed class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Func<Control>> _viewMap;

        public ViewLocator()
        {
            _viewMap = new Dictionary<Type, Func<Control>>()
            {
                [typeof(ProcessingImagesViewModel)] = () => new DragAndDropView(),
                [typeof(DataEntriesViewModel)] = () => new DataEntriesView(),
                [typeof(VirtualizedImagesViewModel)] = () => new VirtualizedImagesView(),
                [typeof(AsyncStateListViewModel)] = () => new AsyncStateListView(),
                [typeof(ProgressViewModel)] = () => new ProgressView(),
                [typeof(BusyViewModel)] = () => new BusyView(),
                [typeof(GeometryRenderViewModel)] = () => new GeometryRenderView(),
                [typeof(PasswordViewModel)] = () => new PasswordView(),
                [typeof(DialogViewModel)] = () => new DialogView(),
                [typeof(ProcessViewModel)] = () => new ProcessView(),
                [typeof(ContextMenuViewModels)] = () => new ContextMenuView(),
                [typeof(EnumsViewModel)] = () => new EnumsView(),
                [typeof(ToastsViewModel)] = () => new ToastsView(),
                [typeof(FormViewModel)] = () => new FormView(),
                [typeof(ObservableDictionaryViewModel)] = () => new ObservableDictionaryView(),
                [typeof(SelectionsViewModel)] = () => new SelectionsView(),
                [typeof(TagCloudViewModel)] = () => new TagCloudView(),
                [typeof(HtmlReadmeViewModel)] = () => new HtmlReadmeView(),
            };
        }

        public Control? Build(object? viewmodel)
        {
            if (viewmodel is null)
            {
                return new TextBlock { Text = "NULL" };
            }

            // factories instead of Activator.CreateInstance, so the views survive trimming and Native AOT
            return _viewMap[viewmodel.GetType()]();
        }

        public bool Match(object? data)
        {
            // only the scenes, this would otherwise take over every viewmodel that is shown as content
            return data is not null && _viewMap.ContainsKey(data.GetType());
        }
    }
}
