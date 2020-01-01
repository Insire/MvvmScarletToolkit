using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DemoApp
{
    public class DataGridViewModel : BusinessViewModelListBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        public DataGridViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Groups = new GroupingViewModel(commandBuilder, () => CollectionViewSource.GetDefaultView(Items), typeof(DataGridRowViewModel));
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 50; i++)
            {
                await Add(new DataGridRowViewModel(CommandBuilder)
                {
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    Color = $"#cccc{i.ToString().PadLeft(2, '0')}",
                });
            }
        }
    }
}
