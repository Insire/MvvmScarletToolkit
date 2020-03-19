namespace MvvmScarletToolkit
{
    public sealed class BoolDialogResultViewModel : DialogResultViewModel<bool>
    {
        public BoolDialogResultViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder, false)
        {
        }

        public BoolDialogResultViewModel(IScarletCommandBuilder commandBuilder, bool model)
            : base(commandBuilder, model)
        {
        }
    }
}
