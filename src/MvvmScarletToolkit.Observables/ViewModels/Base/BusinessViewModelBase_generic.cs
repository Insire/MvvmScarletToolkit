using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// ViewModelBase that bootstraps loading, unloading and refreshing of its content
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BusinessViewModelBase<TModel> : BusinessViewModelBase
        where TModel : class
    {
        private TModel _model;
        [Bindable(true, BindingDirection.OneWay)]
        public TModel Model
        {
            get { return _model; }
            protected set { SetValue(ref _model, value); }
        }

        protected BusinessViewModelBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected BusinessViewModelBase(ICommandBuilder commandBuilder, TModel model)
            : base(commandBuilder)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}
