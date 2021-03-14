using System.Collections.Generic;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public class ImageFactory
    {
        private readonly IScarletCommandBuilder _commandBuilder;

        public ImageFactory(IScarletCommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new System.ArgumentNullException(nameof(commandBuilder));
        }

        public IEnumerable<Image> GetImageList()
        {
            var absolutePath = Path.GetFullPath(".");
            const string resourcesFolder = "Resources";

            yield return new Image(_commandBuilder)
            {
                IsSelected = true,
                DisplayName = "Valley",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-75354.jpg"),
                Sequence = 0,
            };

            yield return new Image(_commandBuilder)
            {
                IsSelected = false,
                DisplayName = "Jungle",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-245035.jpg"),
                Sequence = 1,
            };

            yield return new Image(_commandBuilder)
            {
                IsSelected = false,
                DisplayName = "Winter",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-319605.jpg"),
                Sequence = 2,
            };

            yield return new Image(_commandBuilder)
            {
                IsSelected = false,
                DisplayName = "Night",
                Path = Path.Combine(absolutePath, resourcesFolder, "wallhaven-401406.jpg"),
                Sequence = 3,
            };

            yield return new Image(_commandBuilder)
            {
                IsSelected = false,
                DisplayName = "Moon",
                Path = Path.Combine(absolutePath, resourcesFolder, "Moon_Color_Hypersaturated_Stars_900.jpg"),
                Sequence = 4,
            };

            yield return new Image(_commandBuilder)
            {
                IsSelected = false,
                DisplayName = "Road",
                Path = Path.Combine(absolutePath, resourcesFolder, "Death_to_Stock_Photography_RideorDie_8.jpg"),
                Sequence = 5,
            };
        }
    }
}
