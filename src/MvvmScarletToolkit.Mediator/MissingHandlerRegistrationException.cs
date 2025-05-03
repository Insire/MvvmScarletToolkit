namespace MvvmScarletToolkit.Mediator
{
    public sealed class MissingHandlerRegistrationException : Exception
    {
        public MissingHandlerRegistrationException()
        {
        }

        public MissingHandlerRegistrationException(Type type)
            : base($"No handler registration found for type {type.FullName}")
        {
        }

        public MissingHandlerRegistrationException(string? message) : base(message)
        {
        }

        public MissingHandlerRegistrationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
