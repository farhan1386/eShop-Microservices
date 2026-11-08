using System.ComponentModel.DataAnnotations;

namespace eShop.CustomerService.Models.DTOs
{
    public record CustomerOnboardRequest(
        [Required][EmailAddress] string email,
        [Required][MinLength(8)] string password,
        [Required] string firstName,
        [Required] string lastName,
        string phone,
        [Required] string street,
        [Required] string city,
        [Required] string state,
        [Required] string postalCode
    );
}