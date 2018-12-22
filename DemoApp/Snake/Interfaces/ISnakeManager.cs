﻿using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;

namespace DemoApp
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
        IAsyncCommand PlayCommand { get; }

        /// <summary>
        /// Cancels and resets a game
        /// </summary>
        IAsyncCommand ResetCommand { get; }
    }
}