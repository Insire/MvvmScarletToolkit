using MvvmScarletToolkit;
using System.ComponentModel;

namespace DemoApp
{
    public class Scenes : ViewModelListBase<Scene>
    {
        public Scenes()
        {
            var content = new[]
            {
                new Scene
                {
                    Content = new Default(),
                    DisplayName = "Loading a ParentViewModel",
                    GetDataContext = ()=> new ParentViewModel(),
                    IsSelected = false,
                },

                new Scene
                {
                    Content = new Empty(),
                    DisplayName = "No datacontext loading",
                    IsSelected = false,
                },

                new Scene
                {
                    Content = new ImagesView(),
                    DisplayName = "async image loading",
                    GetDataContext = ()=> ImageFactory.GetImages(),
                    IsSelected = false,
                },

                new Scene
                {
                    Content = new DragAndDrop(),
                    DisplayName = "async image dragging and dropping",
                    GetDataContext = ()=> new ProcessingImagesViewModel(),
                    IsSelected = false,
                },

                new Scene
                {
                    GetDataContext = ()=> new DataContextSchenanigansViewModel(),
                    Content = new DataContextSchenanigans(),
                    DisplayName = "Stuff with content controls and datatemplates",

                    IsSelected = true,
                },
            };

            using (BusyStack.GetToken())
            {
                AddRange(content);

                using (View.DeferRefresh())
                {
                    View.SortDescriptions.Add(new SortDescription(nameof(Scene.DisplayName), ListSortDirection.Ascending));
                }
            }
        }
    }
}
