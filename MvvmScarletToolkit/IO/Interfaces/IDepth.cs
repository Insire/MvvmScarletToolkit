using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IDepth
    {
        int Depth { get; set; }

        int MaxDepth { get; }

        bool CanLoad { get; }
    }
}
