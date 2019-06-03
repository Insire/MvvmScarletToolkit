using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        private readonly LocalizationsViewModel _localizationsViewModel;

        protected Scenes(ICommandBuilder commandBuilder, LocalizationsViewModel localizationsViewModel)
            : base(commandBuilder)
        {
            _localizationsViewModel = localizationsViewModel ?? throw new ArgumentNullException(nameof(LocalizationsViewModel));
        }

        protected void Add(string key, object content)
        {
            var viewmodel = new Scene(CommandBuilder, _localizationsViewModel.CreateViewModel(key))
            {
                Content = content,
                Sequence = Items.Count,
            };

            if (Items.Count == 0)
            {
                viewmodel.IsSelected = true;
            }

            _items.Add(viewmodel);
        }
    }
}
