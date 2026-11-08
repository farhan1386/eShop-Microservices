using System.ComponentModel.DataAnnotations;

namespace eShop.CustomerService.Models.DTOs
{
    public record CustomerUpdateRequest(
        [Required] string firstName,
        [Required] string lastName,
        string phone
    );
}