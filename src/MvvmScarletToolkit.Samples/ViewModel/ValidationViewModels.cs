using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
{
    public sealed class ValidationViewModels : BusinessViewModelListBase<DataGridRowValidationViewModel>
    {
        private readonly DataGridRowViewModelValidator _validator;

        public ValidationViewModels(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _validator = new DataGridRowViewModelValidator();
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 50; i++)
            {
                var vm = new DataGridRowValidationViewModel(_validator, new DataGridRowViewModel(CommandBuilder)
                {
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                });

                await Add(vm).ConfigureAwait(false);
                await vm.Validate().ConfigureAwait(false);
            }
        }
    }
}
