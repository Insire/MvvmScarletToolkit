namespace MvvmScarletToolkit.Observables
{
    public interface IViewModelMerger<TViewModel>
    {
        /// <summary>
        /// Either map and update original instance or replace with new instance. Return result of either
        /// </summary>
        (TViewModel Instance, bool Replace) Perform(TViewModel sourceInstance, TViewModel changedInstance);
    }
}
