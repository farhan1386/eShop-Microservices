namespace eShop.InventoryService.Models.DTOs
{
    public record InitializeStockRequestDto(
        Guid ProductUid,
        int InitialQuantity
    );

    public record StockQuantityRequestDto(
        int Quantity
    );

    public record InventoryStockResponseDto(
        Guid Uid,
        Guid ProductUid,
        int TotalQuantity,
        int ReservedQuantity,
        int AvailableQuantity
    );
}
