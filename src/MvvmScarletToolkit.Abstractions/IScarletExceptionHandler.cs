using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IScarletExceptionHandler
    {
        Task Handle(Exception ex);
    }
}
