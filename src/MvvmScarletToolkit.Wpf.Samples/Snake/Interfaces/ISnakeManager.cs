using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public interface ISnakeManager : INotifyPropertyChanged, IDisposable
    {
        GameState State { get; }

        /// <summary>
        /// Updates boardpieces
        /// </summary>
        void Refresh();

        /// <summary>
        /// Starts a game
        /// </summary>
        IConcurrentCommand PlayCommand { get; }

        /// <summary>
        /// Cancels and resets a game
        /// </summary>
        IConcurrentCommand ResetCommand { get; }
    }
}