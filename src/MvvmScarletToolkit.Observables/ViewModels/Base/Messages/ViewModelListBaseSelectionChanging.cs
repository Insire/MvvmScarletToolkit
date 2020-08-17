namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanging<TViewModel> : GenericScarletMessage<TViewModel>
    {
        public ViewModelListBaseSelectionChanging(in object sender, in TViewModel content)
            : base(sender, content)
        {
        }
    }
}
