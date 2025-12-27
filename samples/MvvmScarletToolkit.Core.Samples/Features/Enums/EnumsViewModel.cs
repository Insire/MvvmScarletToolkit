using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features.Enums
{
    public sealed class EnumsViewModel : EnumsViewModel<ViewModelEnum>
    {
        public EnumsViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder,
            [ViewModelEnum.None,
                ViewModelEnum.AnotherValue,
                ViewModelEnum.SomeValue])
        {
        }
    }
}
