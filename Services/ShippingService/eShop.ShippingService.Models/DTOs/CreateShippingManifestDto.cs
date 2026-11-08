using System;

namespace eShop.ShippingService.Models.DTOs
{
    public record CreateShippingManifestDto(Guid OrderUid, string CarrierName, string? TrackingNumber, string ShippingStatus, DateTime? EstimatedDelivery);
    public record UpdateShippingManifestDto(string? TrackingNumber, string ShippingStatus, DateTime? EstimatedDelivery);
    public record AddShippingHistoryDto(Guid ShippingManifestUid, string OldStatus, string NewStatus, string? StatusLocation, string? Comment);
}