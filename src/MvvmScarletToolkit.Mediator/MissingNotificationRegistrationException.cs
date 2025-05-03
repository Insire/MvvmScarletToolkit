namespace MvvmScarletToolkit.Mediator
{
    public sealed class MissingNotificationRegistrationException : Exception
    {
        public MissingNotificationRegistrationException()
        {
        }

        public MissingNotificationRegistrationException(Type type)
            : base($"No notification registration found for type {type.FullName}")
        {
        }

        public MissingNotificationRegistrationException(string? message) : base(message)
        {
        }

        public MissingNotificationRegistrationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
