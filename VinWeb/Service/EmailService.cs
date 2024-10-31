using SendGrid;
using SendGrid.Helpers.Mail;
using Vin.Services.AuthAPI.Services.IServices;

namespace Vin.Services.AuthAPI.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public SendGridEmailService(
            IConfiguration configuration,
            ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["SendGrid:ApiKey"];
            _senderEmail = _configuration["SendGrid:SenderEmail"];
            _senderName = _configuration["SendGrid:SenderName"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new ArgumentNullException("SendGrid API key is not configured");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_senderEmail, _senderName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            try
            {
                var response = await client.SendEmailAsync(msg);
                var responseBody = await response.Body.ReadAsStringAsync();

                _logger.LogInformation("SendGrid response status code: {StatusCode}", response.StatusCode);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    throw new Exception($"Failed to send email. StatusCode: {response.StatusCode}, ResponseBody: {responseBody}");
                }

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task<string> GetEmailTemplate(string templateName, Dictionary<string, string> replacements)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(basePath, "Templates", "Email", $"{templateName}.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template not found: {templatePath}");
            }

            var template = await File.ReadAllTextAsync(templatePath);

            foreach (var replacement in replacements)
            {
                template = template.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
            }

            return template;
        }
    }
}
