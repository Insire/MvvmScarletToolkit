using MvvmScarletToolkit.Observables;
using System.Reflection;

namespace MvvmScarletToolkit
{
    public sealed class GroupViewModel : ViewModelBase<PropertyInfo>
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        public GroupViewModel(ICommandBuilder commandBuilder, PropertyInfo propertyInfo)
            : base(commandBuilder, propertyInfo)
        {
            Name = Model.Name;
        }
    }
}
