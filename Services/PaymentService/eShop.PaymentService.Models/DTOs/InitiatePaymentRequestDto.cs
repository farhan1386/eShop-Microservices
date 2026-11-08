using eShop.Contracts.Enums;

namespace eShop.PaymentService.Models.DTOs
{
    public record InitiatePaymentRequestDto(
        Guid OrderUid,
        decimal Amount,
        string Currency,
        string Method,
        string GatewayName
    );

    public record CompletePaymentRequestDto(
        string GatewayTxId
    );

    public record UpdatePaymentStatusRequestDto(
        PaymentStatus Status,
        string? GatewayTxId
    );

    public record PaymentResponseDto(
        Guid Uid,
        Guid OrderUid,
        decimal Amount,
        string Currency,
        string Method,
        string GatewayName,
        string Status,
        string? GatewayTxId,
        DateTime? PaymentDate
    );
}