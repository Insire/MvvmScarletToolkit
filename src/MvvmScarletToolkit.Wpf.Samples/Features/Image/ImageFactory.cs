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
            var assembly = typeof(ImageFactory).Assembly;

            var index = 0;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(".jpg"))
                {
                    yield return new Image(_commandBuilder, assembly)
                    {
                        IsSelected = index == 0,
                        DisplayName = Path.GetFileName(name),
                        Path = name,
                        Sequence = index,
                    };

                    index++;
                }
            }
        }
    }
}
