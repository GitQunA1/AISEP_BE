namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
