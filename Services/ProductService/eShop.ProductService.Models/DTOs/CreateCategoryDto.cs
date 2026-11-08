using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.ProductService.Models.DTOs
{
    public record CreateCategoryDto(string Name, Guid? UserUid);
    public record UpdateCategoryDto(string Name, Guid? UserUid);
}
