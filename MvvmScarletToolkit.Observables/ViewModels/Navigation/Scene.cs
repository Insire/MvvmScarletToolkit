using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Observables
{
    public class Scene : ViewModelBase
    {
        private object _content;
        public object Content
        {
            get { return _content; }
            set { SetValue(ref _content, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set { SetValue(ref _sequence, value); }
        }

        private ILocalizationViewModel _displayName;
        public ILocalizationViewModel DisplayName
        {
            get { return _displayName; }
            private set { SetValue(ref _displayName, value); }
        }

        public Scene(ICommandBuilder commandBuilder, ILocalizationViewModel displayName)
            : base(commandBuilder)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }
    }
}
