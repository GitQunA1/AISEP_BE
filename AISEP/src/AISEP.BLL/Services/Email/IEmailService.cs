namespace AISEP.BLL.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);
        Task SendPasswordResetAsync(string toEmail, string userName, string resetLink);
    }
}
