using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Basic "cancellable" generic message
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class CancellableGenericScarletMessage<TContent> : ValueChangedMessage<TContent>
    {
        public Action Cancel { get; }

        public CancellableGenericScarletMessage(in TContent content, in Action cancelAction)
            : base(content)
        {
            Cancel = cancelAction ?? throw new ArgumentNullException(nameof(cancelAction));
        }
    }
}
