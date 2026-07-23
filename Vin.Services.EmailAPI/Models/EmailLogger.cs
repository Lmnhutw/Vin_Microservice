namespace Vin.Services.EmailAPI.Models
{
    public sealed class EmailLogger
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Message { get; set; }
        public DateTime? EmailSent { get; set; }
    }
}
