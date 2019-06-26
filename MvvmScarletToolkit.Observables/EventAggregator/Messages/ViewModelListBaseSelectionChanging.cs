using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{

    public class ViewModelListBaseSelectionChanging<TViewModel> : GenericScarletMessage<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        public ViewModelListBaseSelectionChanging(object sender, TViewModel content)
            : base(sender, content)
        {
        }
    }
}
