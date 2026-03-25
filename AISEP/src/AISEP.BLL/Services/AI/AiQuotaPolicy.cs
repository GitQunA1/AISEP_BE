using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.AI
{
    public static class AiQuotaPolicy
    {
        public static void EnsureAiQuotaNotExceeded(Subscription subscription, Package package)
        {
            if (subscription.UsedAiRequests >= package.MaxAiRequests)
            {
                throw new InvalidOperationException("Exceeded AI Quota");
            }
        }
    }
}
