using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;
using eShop.ProductService.Models.DTOs;
using eShop.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace eShop.ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IEventBus _eventBus;

        public ProductsController(IProductService productService, IEventBus eventBus)
        {
            _productService = productService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            var dtos = products.Select(p => new ProductDto(p.f_uid, p.f_category_uid, p.f_name, p.f_sku, p.f_price, p.f_attributes));
            return Ok(dtos);
        }

        [HttpGet("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetByUid(Guid uid)
        {
            var product = await _productService.GetByUidAsync(uid);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(new ProductDto(product.f_uid, product.f_category_uid, product.f_name, product.f_sku, product.f_price, product.f_attributes));
        }

        [HttpGet("sku/{sku}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetBySku(string sku)
        {
            var product = await _productService.GetBySkuAsync(sku);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(new ProductDto(product.f_uid, product.f_category_uid, product.f_name, product.f_sku, product.f_price, product.f_attributes));
        }

        [HttpGet("category/{categoryUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(Guid categoryUid)
        {
            var products = await _productService.GetByCategoryAsync(categoryUid);
            var dtos = products.Select(p => new ProductDto(p.f_uid, p.f_category_uid, p.f_name, p.f_sku, p.f_price, p.f_attributes));
            return Ok(dtos);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateProductDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            try
            {
                var model = new ProductModel
                {
                    f_category_uid = dto.CategoryUid,
                    f_name = dto.Name,
                    f_sku = dto.Sku,
                    f_price = dto.Price,
                    f_attributes = dto.Attributes
                };

                var created = await _productService.CreateAsync(model);
                return CreatedAtAction(nameof(GetByUid), new { uid = created.f_uid }, created.f_uid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid uid, [FromBody] UpdateProductDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            var model = new ProductModel
            {
                f_category_uid = dto.CategoryUid,
                f_name = dto.Name,
                f_price = dto.Price,
                f_attributes = dto.Attributes
            };

            var updated = await _productService.UpdateAsync(uid, model);
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
            var affectedRows = await _productService.DeleteAsync(uid);
            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}