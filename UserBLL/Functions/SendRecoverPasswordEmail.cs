using BaseModels;
using System.Net;
using System.Net.Mail;

namespace UserBLL.Functions
{
    public static class SendRecoverPasswordEmail
    {
        public static async Task SendEmail(string recipientEmail, string token)
        {
            MailMessage mail = new(PrivateKeys.SendEmailKeys.SenderEmail, recipientEmail)
            {
                Subject = "Alteração de senha",
                Body = $"<h2><a href='{PrivateKeys.SendEmailKeys.Url}/User/RecoverPassword/{token}'>Acesse este link para alterar sua senha</a></h2>",
                IsBodyHtml = true
            };

            // Create a SmtpClient instance and set SMTP server details
            SmtpClient smtpClient = new("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(PrivateKeys.SendEmailKeys.SenderEmail, PrivateKeys.SendEmailKeys.SenderPassword)
            };

            try
            {
                // Send the email
                await smtpClient.SendMailAsync(mail);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
