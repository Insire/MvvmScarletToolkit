namespace MvvmScarletToolkit.Observables
{
    public sealed class ViewModelListBaseSelectionChanged<TViewModel> : GenericScarletMessage<TViewModel>
    {
        public ViewModelListBaseSelectionChanged(in object sender, in TViewModel content)
            : base(sender, content)
        {
        }
    }
}
