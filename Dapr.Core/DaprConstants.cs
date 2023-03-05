namespace Dapr.Core;

public static class DaprConstants
{
    public static class Components
    {
        public const string PubSub = "pubsub";
    }

    public static class Services
    {
        public const string OrderingService = "dapr-ordering-api";
        public const string PaymentService = "dapr-payment-api";
        public const string UsersService = "dapr-users-api";
        public const string AuditService = "dapr-audit-api";
    }
}