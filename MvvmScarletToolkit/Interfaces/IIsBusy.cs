using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    interface IIsBusy
    {
        BusyStack BusyStack { get; }
        bool IsBusy { get; }
    }
}
