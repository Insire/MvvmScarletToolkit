using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ViewModelListBase<AsyncDemoItem>
    {
        public DataContextSchenanigansViewModel()
        {
            for (var i = 0; i < 10; i++)
            {
                Add(new AsyncDemoItem
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
