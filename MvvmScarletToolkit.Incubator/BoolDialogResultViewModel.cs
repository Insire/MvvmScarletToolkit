namespace MvvmScarletToolkit
{
    public sealed class BoolDialogResultViewModel : DialogResultViewModel<bool>
    {
        public BoolDialogResultViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder, false)
        {
        }

        public BoolDialogResultViewModel(ICommandBuilder commandBuilder, bool model)
            : base(commandBuilder, model)
        {
        }
    }
}
