using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        private readonly LocalizationsViewModel _localizationsViewModel;
        private readonly List<IDisposable> _disposeables;

        private bool _disposed;

        protected Scenes(in IScarletCommandBuilder commandBuilder, in LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder)
        {
            _localizationsViewModel = localizationsViewModel ?? throw new ArgumentNullException(nameof(LocalizationsViewModel));
            _disposeables = new List<IDisposable>();
        }

        protected void Add(string key, object content)
        {
            var localization = _localizationsViewModel.CreateViewModel(WeakEventManager, key);
            _disposeables.Add(localization);

            var viewmodel = new Scene(localization)
            {
                Content = content,
                Sequence = Items.Count,
            };

            _items.Add(viewmodel);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _items.Clear();

                for (var i = 0; i < _disposeables.Count; i++)
                {
                    _disposeables[i].Dispose();
                    _disposeables.Clear();
                }
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}
