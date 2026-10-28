using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public enum VirtualizedImageState
    {
        /// <summary>The list never created a container for this item.</summary>
        NotRealized,

        /// <summary>A container is shown, the image is about to be requested.</summary>
        Pending,

        /// <summary>A container is shown and the image is being fetched (memory cache misses only).</summary>
        Loading,

        /// <summary>A container is shown and the image is displayed.</summary>
        Shown,

        /// <summary>The image could not be loaded, this sticks until the image is loaded successfully.</summary>
        Failed,

        /// <summary>The container was virtualized away again, loads in progress got cancelled.</summary>
        Unrealized,
    }

    public sealed partial class VirtualizedImageViewModel : ViewModelBase
    {
        private readonly Action<VirtualizedImageViewModel> _stateChanged;

        public string Id { get; }

        public string Author { get; }

        public Uri Source { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(State))]
        public partial bool IsRealized { get; private set; }

        /// <summary>
        /// Bound to <see cref="AsyncImageLoadingBehavior.State"/>.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(State))]
        public partial ImageLoadState LoadState { get; set; }

        /// <summary>
        /// How often the list created a container for this item.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(State))]
        public partial int RealizedCount { get; private set; }

        public VirtualizedImageState State
        {
            get
            {
                if (LoadState == ImageLoadState.Failed)
                {
                    return VirtualizedImageState.Failed;
                }

                if (!IsRealized)
                {
                    return RealizedCount == 0 ? VirtualizedImageState.NotRealized : VirtualizedImageState.Unrealized;
                }

                return LoadState switch
                {
                    ImageLoadState.Loading => VirtualizedImageState.Loading,
                    ImageLoadState.Loaded => VirtualizedImageState.Shown,
                    _ => VirtualizedImageState.Pending,
                };
            }
        }

        public VirtualizedImageViewModel(IScarletCommandBuilder commandBuilder, string id, string author, Uri source, Action<VirtualizedImageViewModel> stateChanged)
            : base(commandBuilder)
        {
            Id = id;
            Author = author;
            Source = source;
            _stateChanged = stateChanged;
        }

        [RelayCommand]
        private void Realize()
        {
            RealizedCount++;
            IsRealized = true;
        }

        [RelayCommand]
        private void Unrealize()
        {
            IsRealized = false;
        }

        partial void OnIsRealizedChanged(bool value) => _stateChanged(this);

        partial void OnLoadStateChanged(ImageLoadState value) => _stateChanged(this);
    }
}
