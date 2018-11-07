using DemoApp.ViewModel;
using MvvmScarletToolkit;
using System.ComponentModel;
using System.Linq;

namespace DemoApp
{
    public class Scenes : ViewModelListBase<Scene>
    {
        public Scenes()
        {
            var content = new[]
            {
                new DemoApp.Scene
                {
                    Content = new Default(),
                    DisplayName = "Loading a ParentViewModel",
                    GetDataContext = ()=> new ParentViewModel(),
                    IsSelected = true,
                },
                new DemoApp.Scene
                {
                    Content = new Empty(),
                    DisplayName = "No datacontext loading",
                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    Content = new ImagesView(),
                    DisplayName = "async image loading",
                    GetDataContext = ()=> ImageFactory.GetImages(),
                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    Content = new DragAndDrop(),
                    DisplayName = "async image dragging and dropping",
                    GetDataContext = ()=> new ProcessingImagesViewModel(),
                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    GetDataContext = ()=> new DataContextSchenanigansViewModel(),
                    Content = new DataContextSchenanigans(),
                    DisplayName = "Stuff with content controls and datatemplates + lazy loading i guess",

                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    GetDataContext = ()=> new AsyncCommandViewModelStuff(),
                    Content = new AsyncCommands(),
                    DisplayName = "Async Commands",
                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    Content = new SnakeView(),
                    DisplayName = "Snake",
                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    Content = new DataGrids(),
                    DisplayName = "DataGrids",
                    IsSelected = false,
                    GetDataContext = ()=> new ParentsViewModel(),
                },

                new DemoApp.Scene
                {
                    Content = new DemoApp.Controls.Progress(),
                    DisplayName = "Progress",
                    GetDataContext = ()=> new ProgressViewModel(),
                    IsSelected = true,
                }
            };

            using (BusyStack.GetToken())
            {
                AddRange(content);

                SelectedItem = Items.First(p => p.IsSelected);

                using (View.DeferRefresh())
                {
                    View.SortDescriptions.Add(new SortDescription(nameof(Scene.DisplayName), ListSortDirection.Ascending));
                }
            }
        }
    }
}
