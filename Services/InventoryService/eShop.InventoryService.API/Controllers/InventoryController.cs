using eShop.InventoryService.BusinessLogic.Interfaces;
using eShop.InventoryService.Models;
using eShop.InventoryService.Models.DTOs;
using eShop.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace eShop.InventoryService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IEventBus _eventBus;

        public InventoryController(IInventoryService inventoryService, IEventBus eventBus)
        {
            _inventoryService = inventoryService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryStockResponseDto>))]
        public async Task<ActionResult<IEnumerable<InventoryStockResponseDto>>> GetAll()
        {
            var items = await _inventoryService.GetAllAsync();
            var dtos = items.Select(s => new InventoryStockResponseDto(
                s.f_uid,
                s.f_product_uid,
                s.f_quantity,
                s.f_reserved,
                s.AvailableQuantity
            ));

            return Ok(dtos);
        }

        [HttpGet("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryStockResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryStockResponseDto>> GetByUid(Guid uid)
        {
            var stock = await _inventoryService.GetByUidAsync(uid);
            if (stock is null)
            {
                return NotFound();
            }

            return Ok(new InventoryStockResponseDto(
                stock.f_uid,
                stock.f_product_uid,
                stock.f_quantity,
                stock.f_reserved,
                stock.AvailableQuantity
            ));
        }

        [HttpGet("product/{productUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryStockResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryStockResponseDto>> GetByProduct(Guid productUid)
        {
            var stock = await _inventoryService.GetByProductAsync(productUid);
            if (stock is null)
            {
                return NotFound();
            }

            return Ok(new InventoryStockResponseDto(
                stock.f_uid,
                stock.f_product_uid,
                stock.f_quantity,
                stock.f_reserved,
                stock.AvailableQuantity
            ));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Initialize([FromBody] InitializeStockRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var model = new InventoryStockModel
                {
                    f_product_uid = dto.ProductUid,
                    f_quantity = dto.InitialQuantity
                };

                var created = await _inventoryService.InitializeStockAsync(model);
                return CreatedAtAction(nameof(GetByUid), new { uid = created.f_uid }, created.f_uid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("product/{productUid:guid}/restock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restock(Guid productUid, [FromBody] StockQuantityRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _inventoryService.RestockAsync(productUid, dto.Quantity);
                if (updated is null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("product/{productUid:guid}/reserve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reserve(Guid productUid, [FromBody] StockQuantityRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _inventoryService.ReserveStockAsync(productUid, dto.Quantity);
                if (updated is null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("product/{productUid:guid}/release")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Release(Guid productUid, [FromBody] StockQuantityRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _inventoryService.ReleaseReservedStockAsync(productUid, dto.Quantity);
                if (updated is null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("product/{productUid:guid}/deduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmDeduct(Guid productUid, [FromBody] StockQuantityRequestDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _inventoryService.ConfirmDeductStockAsync(productUid, dto.Quantity);
                if (updated is null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid uid)
        {
            var affectedRows = await _inventoryService.DeleteAsync(uid);
            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}