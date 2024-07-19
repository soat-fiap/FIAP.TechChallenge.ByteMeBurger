using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;

namespace FIAP.TechChallenge.ByteMeBurger.Persistence.Dto;

public class ProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Category { get; set; }

    public decimal Price { get; set; }

    public string Images { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public ProductDto()
    {
    }
}
