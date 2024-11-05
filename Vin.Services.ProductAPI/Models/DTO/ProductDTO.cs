﻿using System.ComponentModel.DataAnnotations;

namespace Vin.Services.ProductAPI.Models.DTO
{
    public class ProductDTO
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Range(1000, 100000000)]
        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }
    }
}
