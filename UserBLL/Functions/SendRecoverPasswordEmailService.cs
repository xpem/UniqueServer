using BaseModels;
using BaseModels.Configs;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace UserBLL.Functions
{
    public interface ISendRecoverPasswordEmailService
    {
        Task SendEmail(string recipientEmail, string token);
    }

    public class SendRecoverPasswordEmailService(SendEmailKeys sendEmailKeys) : ISendRecoverPasswordEmailService
    {
        public async Task SendEmail(string recipientEmail, string token)
        {
            string senderEmail = sendEmailKeys.senderEmail;
            string url = sendEmailKeys.url;
            string senderPassword = sendEmailKeys.senderPassword;

            MailMessage mail = new(senderEmail, recipientEmail)
            {
                Subject = "Alteração de senha",
                Body = $"<h2><a href='{url}/User/RecoverPassword/{token}'>Acesse este link para alterar sua senha</a></h2>",
                IsBodyHtml = true
            };

            SmtpClient smtpClient = new(sendEmailKeys.host)
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            try
            {
                // Send the email
                await smtpClient.SendMailAsync(mail);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
