namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanged<TViewModel> : GenericScarletMessage<TViewModel>
    {
        public ViewModelListBaseSelectionChanged(object sender, TViewModel content)
            : base(sender, content)
        {
        }
    }
}
