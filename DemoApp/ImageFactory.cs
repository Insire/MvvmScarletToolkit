using System.Collections.Generic;
using System.IO;

namespace DemoApp
{
    public static class ImageFactory
    {
        public static IEnumerable<Image> GetImageList()
        {
            var absolutePath = Path.GetFullPath(".");
            var resourcesFolder = "Resources";

            yield return new Image()
            {
                IsSelected = true,
                DisplayName = "Valley",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-75354.jpg"),
                Sequence = 0,
            };

            yield return new Image()
            {
                IsSelected = false,
                DisplayName = "Jungle",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-245035.jpg"),
                Sequence = 1,
            };

            yield return new Image()
            {
                IsSelected = false,
                DisplayName = "Winter",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-319605.jpg"),
                Sequence = 2,
            };

            yield return new Image()
            {
                IsSelected = false,
                DisplayName = "Night",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-401406.jpg"),
                Sequence = 3,
            };

            yield return new Image()
            {
                IsSelected = false,
                DisplayName = "Moon",
                Path = Path.Combine(absolutePath, resourcesFolder, "Moon_Color_Hypersaturated_Stars_900.jpg"),
                Sequence = 4,
            };

            yield return new Image()
            {
                IsSelected = false,
                DisplayName = "Road",
                Path = Path.Combine(absolutePath, resourcesFolder, "Death_to_Stock_Photography_RideorDie_8.jpg"),
                Sequence = 5,
            };
        }

        public static Images GetImages()
        {
            var result = new Images();
            result.AddRange(GetImageList());
            return result;
        }
    }
}
