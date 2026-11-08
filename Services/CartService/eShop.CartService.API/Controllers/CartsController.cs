using eShop.CartService.BusinessLogic.Interfaces;
using eShop.CartService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace eShop.CartService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("customer/{customerUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCart(Guid customerUid)
        {
            var cart = await _cartService.GetOrCreateCartAsync(customerUid);
            return Ok(new
            {
                cart.f_uid,
                cart.f_customer_uid,
                cart.f_create_date
            });
        }

        [HttpGet("customer/{customerUid:guid}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetItems(Guid customerUid)
        {
            var items = await _cartService.GetCartItemsAsync(customerUid);
            return Ok(items.Select(item => new
            {
                item.f_uid,
                item.f_cart_uid,
                item.f_product_uid,
                item.f_quantity,
                item.f_price_snapshot,
                item.f_create_date
            }));
        }

        [HttpPost("items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _cartService.AddItemToCartAsync(
                    request.CustomerUid,
                    request.ProductUid,
                    request.Quantity,
                    request.PriceSnapshot);

                if (!success)
                {
                    return BadRequest(new { error = "Failed to add item to cart." });
                }

                return Ok(new { message = "Item added to cart successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("items/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateItemQuantity(Guid id, [FromBody] UpdateCartItemDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.CartItemUid)
            {
                return BadRequest(new { error = "Route identifier mismatch with request body data." });
            }

            try
            {
                var success = await _cartService.UpdateCartItemQuantityAsync(id, request.Quantity);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("items/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            var success = await _cartService.RemoveItemFromCartAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("customer/{customerUid:guid}/clear")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Clear(Guid customerUid)
        {
            var success = await _cartService.ClearCartAsync(customerUid);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}