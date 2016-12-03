using MvvmScarletToolkit;
using System.IO;

namespace DemoApp
{
    public class Images : OrderableViewModelBase<Image>
    {
        public Images(bool filled = false)
        {
            if (!filled)
                return;

            var absolutePath = Path.GetFullPath(".");
            var resourcesFolder = "Resources";

            var content = new[]
            {
                new Image()
                {
                    IsSelected = true,
                    DisplayName = "Valley",
                    Path = Path.Combine(absolutePath,resourcesFolder,"wallhaven-75354.jpg"),
                    Sequence = 0,
                },

                new Image()
                {
                    IsSelected = false,
                    DisplayName = "Jungle",
                    Path = Path.Combine(absolutePath,resourcesFolder,"wallhaven-245035.jpg"),
                    Sequence = 1,
                },

                new Image()
                {
                    IsSelected = false,
                    DisplayName = "Winter",
                    Path = Path.Combine(absolutePath,resourcesFolder,"wallhaven-319605.jpg"),
                    Sequence = 2,
                },

                new Image()
                {
                    IsSelected = false,
                    DisplayName = "Night",
                    Path = Path.Combine(absolutePath,resourcesFolder,"wallhaven-401406.jpg"),
                    Sequence = 3,
                },

                new Image()
                {
                    IsSelected = false,
                    DisplayName = "Moon",
                    Path = Path.Combine(absolutePath,resourcesFolder,"Moon_Color_Hypersaturated_Stars_900.jpg"),
                    Sequence = 4,
                },

                new Image()
                {
                    IsSelected = false,
                    DisplayName = "Road",
                    Path = Path.Combine(absolutePath,resourcesFolder,"Death_to_Stock_Photography_RideorDie_8.jpg"),
                    Sequence = 5,
                },
            };

            Items.AddRange(content);
        }
    }
}
