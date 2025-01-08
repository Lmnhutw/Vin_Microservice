using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vin.Services.ShoppingCartAPI.Models.DTO
{
    public class ProductDTO
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }

        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string? ImageUrl { get; set; }

        [NotMapped]
        public string? ImageLocalPath { get; set; }
    }
}
