using MvvmScarletToolkit;
using System.IO;

namespace DemoApp
{
    public class Images : ViewModelBase<Image>
    {
        public Images()
        {
            var absolutePath = Path.GetFullPath(".");
            var resourcesFolder = "Resources";

            var content = new[]
            {
                new Image()
                {
                    IsSelected=true,
                    DisplayName = "Valley",
                    Path =Path.Combine(absolutePath,resourcesFolder,"wallhaven-75354.jpg"),
                },

                new Image()
                {
                    IsSelected=false,
                    DisplayName="Jungle",
                    Path =Path.Combine(absolutePath,resourcesFolder,"wallhaven-245035.jpg"),
                },

                new Image()
                {
                    IsSelected=false,
                    DisplayName="Winter",
                    Path =Path.Combine(absolutePath,resourcesFolder,"wallhaven-319605.jpg"),
                },

                new Image()
                {
                    IsSelected=false,
                    DisplayName="Night",
                    Path =Path.Combine(absolutePath,resourcesFolder,"wallhaven-401406.jpg"),
                },

                new Image()
                {
                    IsSelected=false,
                    DisplayName="Moon",
                    Path =Path.Combine(absolutePath,resourcesFolder,"Moon_Color_Hypersaturated_Stars_900.jpg"),
                },

                new Image()
                {
                    IsSelected=false,
                    DisplayName="Road",
                    Path =Path.Combine(absolutePath,resourcesFolder,"Death_to_Stock_Photography_RideorDie_8.jpg"),
                },
            };

            Items.AddRange(content);
        }
    }
}
