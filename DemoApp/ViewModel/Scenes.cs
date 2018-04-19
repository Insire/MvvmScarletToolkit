﻿using MvvmScarletToolkit;

using System.ComponentModel;

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
                    IsSelected = false,
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
                    DisplayName = "Stuff with content controls and datatemplates",

                    IsSelected = false,
                },

                new DemoApp.Scene
                {
                    GetDataContext = ()=> new AsyncCommandViewModelStuff(),
                    Content = new AsyncCommands(),
                    DisplayName = "Async Commands",
                    IsSelected = true,
                },

                new DemoApp.Scene
                {

                    Content = new SnakeView(),
                    DisplayName = "Snake",
                    IsSelected = true,
                }
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