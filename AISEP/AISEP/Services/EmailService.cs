using AISEP.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AISEP.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
           
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
           
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                
            
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
        {
            var subject = "AISEP - Xác thực Email của bạn";
            var htmlMessage = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 30px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 30px; 
                            background-color: #4CAF50; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 5px;
                            margin: 20px 0;
                            font-weight: bold;
                        }}
                        .button:hover {{
                            background-color: #45a049;
                        }}
                        .code-box {{
                            background-color: #eee; 
                            padding: 10px; 
                            border-radius: 5px;
                            word-break: break-all;
                            font-family: monospace;
                            font-size: 12px;
                        }}
                        .footer {{ 
                            padding: 20px; 
                            text-align: center; 
                            font-size: 12px; 
                            color: #666; 
                            background-color: #f0f0f0;
                            border-radius: 0 0 5px 5px;
                        }}
                        .warning {{
                            background-color: #fff3cd;
                            border-left: 4px solid #ffc107;
                            padding: 10px;
                            margin: 15px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Chào mừng đến với AISEP!</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {userName},</h2>
                            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>AISEP - AI Startup Ecosystem Platform</strong>.</p>
                            <p>Để hoàn tất quá trình đăng ký và kích hoạt tài khoản, vui lòng xác thực địa chỉ email của bạn bằng cách click vào nút bên dưới:</p>
                            <div style='text-align: center;'>
                                <a href='{confirmationLink}' class='button'>✅ Xác thực Email</a>
                            </div>
                            <p>Hoặc bạn có thể copy và paste link sau vào trình duyệt:</p>
                            <div class='code-box'>{confirmationLink}</div>
                            <div class='warning'>
                                <strong>⚠️ Lưu ý quan trọng:</strong>
                                <ul style='margin: 10px 0; padding-left: 20px;'>
                                    <li>Link xác thực này sẽ <strong>hết hạn sau 24 giờ</strong></li>
                                    <li>Nếu link hết hạn, bạn có thể yêu cầu gửi lại email xác thực</li>
                                    <li>Không chia sẻ link này với bất kỳ ai</li>
                                </ul>
                            </div>
                            <p>Nếu bạn không thực hiện đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                            <p>Trân trọng,<br><strong>Đội ngũ AISEP</strong></p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 AISEP - AI Startup Ecosystem Platform. All rights reserved.</p>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendPasswordResetAsync(string toEmail, string userName, string resetLink)
        {
            var subject = "AISEP - Đặt lại mật khẩu";
            var htmlMessage = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 30px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 30px; 
                            background-color: #f44336; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 5px;
                            margin: 20px 0;
                            font-weight: bold;
                        }}
                        .button:hover {{
                            background-color: #da190b;
                        }}
                        .code-box {{
                            background-color: #eee; 
                            padding: 10px; 
                            border-radius: 5px;
                            word-break: break-all;
                            font-family: monospace;
                            font-size: 12px;
                        }}
                        .warning {{ 
                            background-color: #fff3cd; 
                            border-left: 4px solid #ffc107; 
                            padding: 15px; 
                            margin: 15px 0; 
                        }}
                        .footer {{ 
                            padding: 20px; 
                            text-align: center; 
                            font-size: 12px; 
                            color: #666; 
                            background-color: #f0f0f0;
                            border-radius: 0 0 5px 5px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Đặt lại mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {userName},</h2>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản AISEP của bạn.</p>
                            <p>Click vào nút bên dưới để tiến hành đặt lại mật khẩu:</p>
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='button'>🔑 Đặt lại mật khẩu</a>
                            </div>
                            <p>Hoặc copy và paste link sau vào trình duyệt:</p>
                            <div class='code-box'>{resetLink}</div>
                            <div class='warning'>
                                <strong>⚠️ Lưu ý bảo mật:</strong>
                                <ul style='margin: 10px 0; padding-left: 20px;'>
                                    <li>Link này <strong>chỉ có hiệu lực trong 1 giờ</strong></li>
                                    <li>Nếu bạn <strong>KHÔNG</strong> yêu cầu đặt lại mật khẩu, vui lòng <strong>BỎ QUA</strong> email này và liên hệ với chúng tôi ngay</li>
                                    <li><strong>TUYỆT ĐỐI KHÔNG</strong> chia sẻ link này với bất kỳ ai</li>
                                    <li>Link chỉ sử dụng được <strong>MỘT LẦN</strong></li>
                                </ul>
                            </div>
                            <p>Sau khi đặt lại mật khẩu thành công, bạn sẽ cần đăng nhập lại với mật khẩu mới.</p>
                            <p>Trân trọng,<br><strong>Đội ngũ AISEP</strong></p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 AISEP - AI Startup Ecosystem Platform. All rights reserved.</p>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }
    }
}
