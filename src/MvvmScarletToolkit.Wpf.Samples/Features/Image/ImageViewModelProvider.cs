using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ImageViewModelProvider
    {
        private readonly string[] _urls = new string[]
        {
            "https://w.wallhaven.cc/full/o5/wallhaven-o58767.jpg",
            "https://w.wallhaven.cc/full/1p/wallhaven-1pyl9g.jpg",
            "https://w.wallhaven.cc/full/jx/wallhaven-jx7y95.jpg",
            "https://w.wallhaven.cc/full/9d/wallhaven-9dpejk.jpg",
            "https://w.wallhaven.cc/full/6d/wallhaven-6d6pvl.jpg",
        };

        private readonly IScarletCommandBuilder _commandBuilder;
        private readonly EnvironmentInformationProvider _environmentInformationProvider;
        private int _index;

        public ImageViewModelProvider(IScarletCommandBuilder commandBuilder, EnvironmentInformationProvider environmentInformationProvider)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _environmentInformationProvider = environmentInformationProvider;
        }

        public IEnumerable<ImageViewModel> GetImages()
        {
            return GetLocalImages().Interleave(GetWebImages());
        }

        private IEnumerable<ImageViewModel> GetLocalImages()
        {
            var directoryPath = _environmentInformationProvider.GetResourceFolderPath();

            foreach (var filePath in Directory.GetFiles(directoryPath, searchPattern: "*.jpg", SearchOption.TopDirectoryOnly))
            {
                yield return new ImageViewModel(_commandBuilder)
                {
                    IsSelected = _index == 0,
                    DisplayName = Path.GetFileName(filePath),
                    Path = filePath,
                    Sequence = _index,
                };

                _index++;
            }
        }

        private IEnumerable<ImageViewModel> GetWebImages()
        {
            for (var i = 0; i < _urls.Length; i++)
            {
                yield return new ImageViewModel(_commandBuilder)
                {
                    IsSelected = _index == 0,
                    DisplayName = $"Image {i}",
                    Path = _urls[i],
                    Sequence = _index,
                };
            }
        }
    }
}
