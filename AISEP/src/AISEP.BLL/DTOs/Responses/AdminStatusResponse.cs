namespace AISEP.BLL.DTOs.Responses
{
    public class AdminStatusResponse
    {
        public ServiceStatusResponse Ai { get; set; } = new();
        public ServiceStatusResponse Blockchain { get; set; } = new();
    }
}
