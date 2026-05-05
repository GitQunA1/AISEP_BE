using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class SubscriptionCheckoutRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "PackageId phải lớn hơn 0.")]
        public int PackageId { get; set; }
    }
}
