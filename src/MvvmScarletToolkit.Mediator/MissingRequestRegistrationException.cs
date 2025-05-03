namespace MvvmScarletToolkit.Mediator
{
    public sealed class MissingRequestRegistrationException : Exception
    {
        public MissingRequestRegistrationException()
        {
        }

        public MissingRequestRegistrationException(Type type)
            : base($"No request registration found for type {type.FullName}")
        {
        }

        public MissingRequestRegistrationException(string? message) : base(message)
        {
        }

        public MissingRequestRegistrationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
