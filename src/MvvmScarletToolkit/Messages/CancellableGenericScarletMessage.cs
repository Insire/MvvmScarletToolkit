using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Basic "cancellable" generic message
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class CancellableGenericScarletMessage<TContent> : GenericScarletMessage<TContent>
    {
        public Action Cancel { get; }

        public CancellableGenericScarletMessage(in object sender, in TContent content, in Action cancelAction)
            : base(sender, content)
        {
            Cancel = cancelAction ?? throw new ArgumentNullException(nameof(cancelAction));
        }
    }
}
