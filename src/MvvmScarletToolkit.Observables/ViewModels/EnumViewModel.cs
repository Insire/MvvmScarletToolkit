using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MvvmScarletToolkit.Observables
{
    public static class EnumViewModel
    {
        public static EnumViewModel<TEnum> Create<TEnum>(in TEnum value, in IMessenger messenger)
            where TEnum : Enum
        {
            return new EnumViewModel<TEnum>(value, value.GetAttributeOfType<DescriptionAttribute>()?.Description ?? value.ToString(), messenger);
        }
    }

    public abstract class EnumsViewModel<TEnum> : ViewModelListBase<EnumViewModel<TEnum>>
        where TEnum : Enum
    {
        protected EnumsViewModel(IScarletCommandBuilder commandBuilder, IEnumerable<TEnum> values)
            : base(commandBuilder)
        {
            EnumViewModel<TEnum>? first = null;
            foreach (var value in values)
            {
                if (first == null)
                {
                    first = AddUnchecked(EnumViewModel.Create(value, Messenger));
                }
                else
                {
                    AddUnchecked(EnumViewModel.Create(value, Messenger));
                }
            }

            SelectedItem = first;

            Messenger.Register<EnumsViewModel<TEnum> ,PropertyChangedMessage<bool>>(this, (r, m) =>
            {
                if(m.Sender is  EnumViewModel<TEnum> { IsSelected: true } vm)
                {
                    r.SelectedItem = vm;
                }
            });
        }
    }
}
