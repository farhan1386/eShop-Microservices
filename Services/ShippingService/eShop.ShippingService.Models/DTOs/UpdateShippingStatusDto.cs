using System;

namespace eShop.ShippingService.Models.DTOs
{
    public record UpdateShippingStatusDto(
        string NewStatus,
        string? StatusLocation,
        string? Comment,
        string? TrackingNumber,
        DateTime? EstimatedDelivery
    );
}