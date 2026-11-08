using eShop.Contracts.Enums;
using System;
using System.Collections.Generic;

namespace eShop.OrderService.Models.DTOs
{
    public record OrderItemRequestDto(
        Guid ProductUid,
        int Quantity,
        decimal UnitPrice
    );

    public record PlaceOrderRequestDto(
        Guid CustomerUid,
        List<OrderItemRequestDto> Items
    );

    public record UpdateOrderStatusRequestDto(
        OrderStatus Status,
        string? Comment
    );

    public record OrderItemResponseDto(
        Guid Uid,
        Guid ProductUid,
        int Quantity,
        decimal UnitPrice
    );

    public record OrderHistoryResponseDto(
         Guid Uid,
         string OldStatus,
         string NewStatus,
         string? Comment,
         DateTime? CreateDate
     );

    public record OrderResponseDto(
        Guid Uid,
        Guid CustomerUid,
        DateTime OrderDate,
        decimal TotalPrice,
        string Status
    );
}