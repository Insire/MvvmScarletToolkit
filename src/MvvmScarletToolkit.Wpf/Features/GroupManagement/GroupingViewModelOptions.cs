using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Reflection;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Limits the properties a <see cref="GroupingViewModel"/> offers for grouping. Without options, every public readable property is offered.
    /// </summary>
    public sealed class GroupingViewModelOptions
    {
        /// <summary>
        /// Hides properties declared by the toolkit base classes, like <see cref="ViewModelBase.IsBusy"/>
        /// </summary>
        public bool ExcludeToolkitProperties { get; init; }

        /// <summary>
        /// When set, only these properties are offered
        /// </summary>
        public IReadOnlyCollection<string>? Properties { get; init; }

        internal bool IsIncluded(PropertyInfo property)
        {
            if (Properties?.Contains(property.Name) == false)
            {
                return false;
            }

            if (!ExcludeToolkitProperties)
            {
                return true;
            }

            var assembly = property.DeclaringType?.Assembly;

            return assembly != typeof(ViewModelBase).Assembly
                && assembly != typeof(ObservableObject).Assembly;
        }
    }
}
