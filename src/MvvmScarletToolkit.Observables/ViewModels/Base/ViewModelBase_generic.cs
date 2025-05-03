using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Generic version of <see cref="ViewModelBase"/> exposing an injected (model)class
    /// </summary>
    public abstract class ViewModelBase<TModel> : ViewModelBase
        where TModel : class
    {
        private TModel? _model;
        [Bindable(true, BindingDirection.OneWay)]
        public TModel? Model
        {
            get { return _model; }
            protected set { SetProperty(ref _model, value); }
        }

        protected ViewModelBase(in IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected ViewModelBase(in IScarletCommandBuilder commandBuilder, in TModel? model)
            : this(commandBuilder)
        {
            _model = model;
        }
    }
}
