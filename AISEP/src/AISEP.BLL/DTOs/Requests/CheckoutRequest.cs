using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class CheckoutRequest
    {
        public ReferenceType ReferenceType { get; set; }  // Subscription or Booking
        public int ReferenceId { get; set; }               // PackageId or BookingId
    }
}
