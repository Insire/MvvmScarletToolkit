using MvvmScarletToolkit.Observables;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class GroupViewModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetProperty(ref _name, value); }
        }

        public PropertyGroupDescription GroupDescription { get; }

        public GroupViewModel(IScarletCommandBuilder commandBuilder, PropertyInfo propertyInfo)
            : base(commandBuilder)
        {
            if (propertyInfo is null)
            {
                throw new System.ArgumentNullException(nameof(propertyInfo));
            }

            _name = propertyInfo.Name;
            GroupDescription = new PropertyGroupDescription(propertyInfo.Name);
        }

        private string GetDebuggerDisplay()
        {
            return $"{nameof(GroupViewModel)}: {Name}";
        }
    }
}
