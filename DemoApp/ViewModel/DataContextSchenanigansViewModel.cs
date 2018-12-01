using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ViewModelListBase<AsyncDemoItem>
    {
        public DataContextSchenanigansViewModel(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
            for (var i = 0; i < 10; i++)
            {
                _ = Add(new AsyncDemoItem
                {
                    DisplayName = "Test X",
                });
            }

            SelectedItem = new AsyncDemoItem
            {
                DisplayName = "Test X",
            };
        }
    }
}
