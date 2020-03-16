using MvvmScarletToolkit.Abstractions;
using System;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Default "pass through" proxy.</para>
    /// <para>Does nothing other than deliver the message.</para>
    /// </summary>
    public sealed class ScarletMessageProxy : IScarletMessageProxy
    {
        private static readonly Lazy<ScarletMessageProxy> _default = new Lazy<ScarletMessageProxy>(() => new ScarletMessageProxy());
        public static IScarletMessageProxy Default => _default.Value;

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);

#if DEBUG

            Debug.WriteLine("Deliver --> " + GetTypeName(message.GetType()) + " via " + GetTypeName(subscription.GetType()));

            static string GetTypeName(Type type)
            {
                var result = type.Name;

                while (type.GenericTypeArguments.Length > 0)
                {
                    var typeName = type.GenericTypeArguments[0].Name;
                    result = result.Replace("`1", $"<{typeName}>");
                    type = type.GenericTypeArguments[0];
                }

                return result;
            }
#endif
        }
    }
}
