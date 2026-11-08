using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.ProductService.Models.DTOs
{
    public record ProductDto(Guid Uid, Guid CategoryUid, string Name, string Sku, decimal Price, string Attributes);
    public record CategoryDto(Guid Uid, string Name);
    public record CreateProductDto(Guid CategoryUid, string Name, string Sku, decimal Price, string Attributes, Guid? UserUid);
    public record UpdateProductDto(Guid CategoryUid, string Name, decimal Price, string Attributes, Guid? UserUid);
}
