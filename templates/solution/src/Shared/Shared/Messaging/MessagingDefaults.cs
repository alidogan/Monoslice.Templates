namespace Shared.Messaging;

public static class MessagingDefaults
{
#if (UseBroker)
    /// <summary>Connection string of the message broker.</summary>
    public const string ConnectionStringName = "messaging";

#endif
    public static bool IsIntegrationEvent(Type type) =>
        type.IsAssignableTo(typeof(Shared.Contracts.Messaging.IIntegrationEvent));
}
