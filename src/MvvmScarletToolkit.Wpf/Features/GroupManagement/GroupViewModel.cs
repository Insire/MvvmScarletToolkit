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
            get => _name;
            private set => SetProperty(ref _name, value);
        }

        public PropertyGroupDescription GroupDescription { get; }

        /// <summary>
        /// the <see cref="GroupingViewModel"/> this instance belongs to.
        /// All viewmodels share a messenger, so this keeps separate groupings from reacting to each other
        /// </summary>
        internal GroupingViewModel? Owner { get; }

        public GroupViewModel(IScarletCommandBuilder commandBuilder, PropertyInfo propertyInfo)
            : this(commandBuilder, propertyInfo, null)
        {
        }

        internal GroupViewModel(IScarletCommandBuilder commandBuilder, PropertyInfo propertyInfo, GroupingViewModel? owner)
            : base(commandBuilder)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            _name = propertyInfo.Name;
            GroupDescription = new PropertyGroupDescription(propertyInfo.Name);
            Owner = owner;
        }

        private string GetDebuggerDisplay()
        {
            return $"{nameof(GroupViewModel)}: {Name}";
        }
    }
}
