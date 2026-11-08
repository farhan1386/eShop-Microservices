using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;
using eShop.ProductService.Models.DTOs;
using eShop.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace eShop.ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IProductCategoryService _categoryService;
        private readonly IEventBus _eventBus;

        public CategoriesController(IProductCategoryService categoryService, IEventBus eventBus)
        {
            _categoryService = categoryService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryDto>))]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            var dtos = categories.Select(c => new CategoryDto(c.f_uid, c.f_name));
            return Ok(dtos);
        }

        [HttpGet("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDto>> GetByUid(Guid uid)
        {
            var category = await _categoryService.GetByUidAsync(uid);
            if (category is null)
            {
                return NotFound();
            }

            return Ok(new CategoryDto(category.f_uid, category.f_name));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCategoryDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            var model = new ProductCategory
            {
                f_name = dto.Name
            };

            var created = await _categoryService.CreateAsync(model);
            return CreatedAtAction(nameof(GetByUid), new { uid = created.f_uid }, created.f_uid);
        }

        [HttpPut("{uid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid uid, [FromBody] UpdateCategoryDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            var model = new ProductCategory
            {
                f_name = dto.Name
            };

            var updated = await _categoryService.UpdateAsync(uid, model);
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
            var affectedRows = await _categoryService.DeleteAsync(uid);
            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}