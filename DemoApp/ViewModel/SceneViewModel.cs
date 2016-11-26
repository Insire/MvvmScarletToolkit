using MvvmScarletToolkit;

namespace DemoApp
{
    public class SceneViewModel : ViewModelBase<Scene>
    {
        public SceneViewModel()
        {
            var content = new[]
            {
                new Scene
                {
                    Content = new Default(),
                    DisplayName = "Loading a ParentViewModel",
                    GetDataContext = ()=> new ParentViewModel(),
                    IsSelected = true,
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
                    GetDataContext = ()=> new Images(),
                    IsSelected = false,
                },
            };

            AddRange(content);
        }
    }
}
