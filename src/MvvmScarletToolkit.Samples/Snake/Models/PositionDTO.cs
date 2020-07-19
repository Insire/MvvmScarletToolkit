using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit.Samples
{
    [DebuggerDisplay("PositionModel {CurrentPosition.X};{CurrentPosition.Y}")]
    internal class PositionDTO : IPositionable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Position _currentPosition;
        public Position CurrentPosition
        {
            get
            {
                return _currentPosition;
            }

            set
            {
                if (ReferenceEquals(value, _currentPosition))
                {
                    return;
                }

                _currentPosition = value;
                OnPropertyChanged();
            }
        }

        public Size Size { get; }

        public PositionDTO(IPositionable positionable)
        {
            _currentPosition = positionable.CurrentPosition;
            Size = positionable.Size;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
