namespace Vin.Services.AuthAPI.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task<string> GetEmailTemplate(string templateName, Dictionary<string, string> replacements);
    }
}
