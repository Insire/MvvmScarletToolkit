namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanging<TViewModel> : GenericScarletMessage<TViewModel>
    {
        public ViewModelListBaseSelectionChanging(object sender, TViewModel content)
            : base(sender, content)
        {
        }
    }
}
