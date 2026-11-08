using eShop.Contracts.Enums;
using eShop.Contracts.Events;
using eShop.EventBus;
using eShop.PaymentService.BusinessLogic.Interfaces;
using eShop.PaymentService.Models;
using eShop.PaymentService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IEventBus _eventBus;

        public PaymentsController(IPaymentService paymentService, IEventBus eventBus)
        {
            _paymentService = paymentService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PaymentResponseDto>))]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAll()
        {
            var payments = await _paymentService.GetAllAsync();
            var dtos = payments.Select(p => new PaymentResponseDto(
                p.f_uid,
                p.f_order_uid,
                p.f_amount,
                p.f_currency,
                p.f_method,
                p.f_gateway_name,
                p.f_status.ToString(),
                p.f_gateway_txid,
                p.f_payment_date
            ));

            return Ok(dtos);
        }

        [HttpGet("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentResponseDto>> GetByUid(Guid uid)
        {
            var payment = await _paymentService.GetByUidAsync(uid);
            if (payment is null)
            {
                return NotFound();
            }

            return Ok(new PaymentResponseDto(
                payment.f_uid,
                payment.f_order_uid,
                payment.f_amount,
                payment.f_currency,
                payment.f_method,
                payment.f_gateway_name,
                payment.f_status.ToString(),
                payment.f_gateway_txid,
                payment.f_payment_date
            ));
        }

        [HttpGet("order/{orderUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PaymentResponseDto>))]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetByOrder(Guid orderUid)
        {
            var payments = await _paymentService.GetByOrderAsync(orderUid);
            var dtos = payments.Select(p => new PaymentResponseDto(
                p.f_uid,
                p.f_order_uid,
                p.f_amount,
                p.f_currency,
                p.f_method,
                p.f_gateway_name,
                p.f_status.ToString(),
                p.f_gateway_txid,
                p.f_payment_date
            ));

            return Ok(dtos);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Initiate([FromBody] InitiatePaymentRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var model = new PaymentModel
                {
                    f_order_uid = dto.OrderUid,
                    f_amount = dto.Amount,
                    f_currency = dto.Currency,
                    f_method = dto.Method,
                    f_gateway_name = dto.GatewayName
                };

                var created = await _paymentService.InitiatePaymentAsync(model);
                return CreatedAtAction(nameof(GetByUid), new { uid = created.f_uid }, created.f_uid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{uid:guid}/complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Complete(Guid uid, [FromBody] CompletePaymentRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.GatewayTxId))
            {
                return BadRequest();
            }

            var updated = await _paymentService.CompletePaymentAsync(uid, dto.GatewayTxId);
            if (updated is null)
            {
                return NotFound();
            }

            await _eventBus.PublishAsync(new PaymentCompletedEvent
            {
                f_uid = Guid.NewGuid(),
                f_create_date = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid(),
                PaymentId = updated.f_uid,
                OrderId = updated.f_order_uid,
                Amount = updated.f_amount,
                Status = (PaymentStatus)updated.f_status
            });

            return NoContent();
        }

        [HttpPut("{uid:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid uid, [FromBody] UpdatePaymentStatusRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            var updated = await _paymentService.UpdateStatusAsync(uid, dto.Status, dto.GatewayTxId);
            if (updated is null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid uid)
        {
            var affectedRows = await _paymentService.DeleteAsync(uid);
            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}