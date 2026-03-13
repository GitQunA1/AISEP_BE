using AISEP.BLL.DTOs.Requests;

namespace AISEP.BLL.Services.Payments
{
    public interface IPaymentService
    {
        Task ProcessSePayWebhookAsync(SePayWebhookRequest request);
    }
}
