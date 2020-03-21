using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Generic ViewModelBase that provides common services required for MVVM
    /// </summary>
    public abstract class ViewModelBase<TModel> : ViewModelBase
        where TModel : class
    {
        private TModel? _model;
        [Bindable(true, BindingDirection.OneWay)]
        public TModel? Model
        {
            get { return _model; }
            protected set { SetValue(ref _model, value); }
        }

        protected ViewModelBase(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected ViewModelBase(IScarletCommandBuilder commandBuilder, TModel model)
            : this(commandBuilder)
        {
            _model = model;
        }
    }
}
