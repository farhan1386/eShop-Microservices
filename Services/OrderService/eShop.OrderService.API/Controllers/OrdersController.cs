using eShop.Contracts.Enums;
using eShop.Contracts.Events;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;
using eShop.OrderService.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderResponseDto>))]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            var dtos = orders.Select(o => new OrderResponseDto(
                o.f_uid,
                o.f_customer_uid,
                o.f_order_date,
                o.f_total_price,
                o.f_status.ToString()
            ));

            return Ok(dtos);
        }

        [HttpGet("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponseDto>> GetByUid(Guid uid)
        {
            var order = await _orderService.GetByUidAsync(uid);
            if (order is null)
            {
                return NotFound();
            }

            return Ok(new OrderResponseDto(
                order.f_uid,
                order.f_customer_uid,
                order.f_order_date,
                order.f_total_price,
                order.f_status.ToString()
            ));
        }

        [HttpGet("customer/{customerUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderResponseDto>))]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByCustomer(Guid customerUid)
        {
            var orders = await _orderService.GetByCustomerAsync(customerUid);
            var dtos = orders.Select(o => new OrderResponseDto(
                o.f_uid,
                o.f_customer_uid,
                o.f_order_date,
                o.f_total_price,
                o.f_status.ToString()
            ));

            return Ok(dtos);
        }

        [HttpGet("{uid:guid}/items")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderItemResponseDto>))]
        public async Task<ActionResult<IEnumerable<OrderItemResponseDto>>> GetItems(Guid uid)
        {
            var items = await _orderService.GetItemsByOrderAsync(uid);
            var dtos = items.Select(i => new OrderItemResponseDto(
                i.f_uid,
                i.f_product_uid,
                i.f_quantity,
                i.f_unit_price
            ));

            return Ok(dtos);
        }

        [HttpGet("{uid:guid}/history")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderHistoryResponseDto>))]
        public async Task<ActionResult<IEnumerable<OrderHistoryResponseDto>>> GetHistory(Guid uid)
        {
            var history = await _orderService.GetHistoryByOrderAsync(uid);
            var dtos = history.Select(h => new OrderHistoryResponseDto(
                h.f_uid,
                h.f_old_status,
                h.f_new_status,
                h.f_comment,
                h.f_create_date
            ));

            return Ok(dtos);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> PlaceOrder([FromBody] PlaceOrderRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var order = new OrderModel
                {
                    f_customer_uid = dto.CustomerUid
                };

                var items = dto.Items.Select(i => new OrderItemModel
                {
                    f_product_uid = i.ProductUid,
                    f_quantity = i.Quantity,
                    f_unit_price = i.UnitPrice
                }).ToList();

                var created = await _orderService.PlaceOrderAsync(order, items);

                return CreatedAtAction(nameof(GetByUid), new { uid = created.f_uid }, created.f_uid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{uid:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid uid, [FromBody] UpdateOrderStatusRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            var updated = await _orderService.UpdateStatusAsync(uid, dto.Status, dto.Comment);
            if (updated is null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(Guid uid, [FromQuery] string? comment = null)
        {
            var affectedRows = await _orderService.CancelOrderAsync(uid, comment);
            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}