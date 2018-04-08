using System;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface ISnakeManager : IDisposable
    {
        /// <summary>
        /// Updates boardpieces
        /// </summary>
        void Refresh();

        /// <summary>
        /// Starts a game
        /// </summary>
        ICommand PlayCommand { get; }

        /// <summary>
        /// Cancels and resets a game
        /// </summary>
        ICommand ResetCommand { get; }
    }
}
