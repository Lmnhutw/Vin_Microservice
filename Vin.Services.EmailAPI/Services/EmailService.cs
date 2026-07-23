using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Vin.Services.EmailAPI.Data;
using Vin.Services.EmailAPI.Models;
using Vin.Services.EmailAPI.Models.Dto;

namespace Vin.Services.EmailAPI.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IDbContextFactory<AppDbContext> dbContextFactory,
            ILogger<EmailService> logger)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(CartDTO cartDto)
        {
            ArgumentNullException.ThrowIfNull(cartDto);

            if (cartDto.CartHeader is null || string.IsNullOrWhiteSpace(cartDto.CartHeader.Email))
            {
                throw new ArgumentException(
                    "CartHeader and CartHeader.Email are required to create an email notification.",
                    nameof(cartDto));
            }

            var message = BuildEmailMessage(cartDto);
            await LogEmailAsync(message, cartDto.CartHeader.Email, cartDto.CartHeader.CartHeaderId);
        }

        private static string BuildEmailMessage(CartDTO cartDto)
        {
            var message = new StringBuilder();
            message.AppendLine("<br/>Cart Email Requested");
            message.AppendLine($"<br/>Total {cartDto.CartHeader!.CartTotal:C}");
            message.Append("<br/><ul>");

            if (cartDto.CartDetails is not null)
            {
                foreach (var item in cartDto.CartDetails)
                {
                    var productName = WebUtility.HtmlEncode(item.Product?.Name ?? "Unknown product");
                    message.Append($"<li>{productName} x {item.Count}</li>");
                }
            }

            message.Append("</ul>");
            return message.ToString();
        }

        private async Task LogEmailAsync(string message, string email, int cartHeaderId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.EmailLoggers.Add(new EmailLogger
            {
                Email = email,
                EmailSent = DateTime.UtcNow,
                Message = message
            });
            await db.SaveChangesAsync();

            // The delivery provider is intentionally not configured in this local build.
            _logger.LogInformation(
                "Email notification logged for cart {CartHeaderId}",
                cartHeaderId);
        }
    }
}
