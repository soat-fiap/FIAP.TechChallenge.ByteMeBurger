using System.ComponentModel.DataAnnotations;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;

namespace FIAP.TechChallenge.ByteMeBurger.Api.Model;

public class ProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    
    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public string[] Images { get; set; } = Array.Empty<string>();

    public ProductDto()
    {
        
    }
    
    public ProductDto(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        Category = product.Category;
        Price = product.Price;
        Images = product.Images.ToArray();
    }
}