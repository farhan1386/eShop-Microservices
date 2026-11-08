using eShop.Contracts.Events;
using eShop.EventBus;
using eShop.ShippingService.BusinessLogic.Interfaces;
using eShop.ShippingService.Models;
using eShop.ShippingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace eShop.ShippingService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly IEventBus _eventBus;

        public ShippingController(IShippingService shippingService, IEventBus eventBus)
        {
            _shippingService = shippingService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var manifests = await _shippingService.GetAllManifestsAsync();

            return Ok(manifests.Select(manifest => new
            {
                manifest.f_uid,
                manifest.f_order_uid,
                manifest.f_carrier_name,
                manifest.f_tracking_number,
                manifest.f_shipping_status,
                manifest.f_estimated_delivery,
                manifest.f_create_date
            }));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var manifest = await _shippingService.GetManifestAsync(id);
            if (manifest == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                manifest.f_uid,
                manifest.f_order_uid,
                manifest.f_carrier_name,
                manifest.f_tracking_number,
                manifest.f_shipping_status,
                manifest.f_estimated_delivery,
                manifest.f_create_date
            });
        }

        [HttpGet("order/{orderUid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByOrderUid(Guid orderUid)
        {
            var manifest = await _shippingService.GetManifestByOrderAsync(orderUid);
            if (manifest == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                manifest.f_uid,
                manifest.f_order_uid,
                manifest.f_carrier_name,
                manifest.f_tracking_number,
                manifest.f_shipping_status,
                manifest.f_estimated_delivery,
                manifest.f_create_date
            });
        }

        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var history = await _shippingService.GetTrackingHistoryAsync(id);

            return Ok(history.Select(item => new
            {
                item.f_uid,
                item.f_shipping_manifest_uid,
                item.f_old_status,
                item.f_new_status,
                item.f_status_location,
                item.f_comment,
                item.f_create_date
            }));
        }

        [HttpPost("initiate")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Initiate([FromBody] CreateShippingManifestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var manifest = new ShippingModel
                {
                    f_order_uid = request.OrderUid,
                    f_carrier_name = request.CarrierName,
                    f_tracking_number = request.TrackingNumber,
                    f_shipping_status = request.ShippingStatus,
                    f_estimated_delivery = request.EstimatedDelivery
                };

                var initialHistory = new ShippingHistoryModel
                {
                    f_old_status = "None",
                    f_new_status = request.ShippingStatus,
                    f_status_location = "Origin Warehouse",
                    f_comment = "Shipping manifest initialized."
                };

                var success = await _shippingService.InitiateShippingAsync(manifest, initialHistory);
                if (!success)
                {
                    return BadRequest(new { error = "Shipping initialization failed." });
                }

                await _eventBus.PublishAsync(new ShippingInitiatedEvent
                {
                    f_uid = Guid.NewGuid(),
                    f_create_date = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    ShippingManifestId = manifest.f_uid,
                    OrderUid = manifest.f_order_uid,
                    CarrierName = manifest.f_carrier_name,
                    TrackingNumber = manifest.f_tracking_number,
                    Status = manifest.f_shipping_status
                });

                return CreatedAtAction(nameof(GetById), new { id = manifest.f_uid }, new
                {
                    shippingManifestUid = manifest.f_uid,
                    message = "Shipping workflow initiated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShippingStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _shippingService.UpdateTrackingStateAsync(
                id,
                request.NewStatus,
                request.StatusLocation,
                request.Comment,
                request.TrackingNumber,
                request.EstimatedDelivery);

            if (!success)
            {
                return NotFound();
            }

            await _eventBus.PublishAsync(new ShippingStatusUpdatedEvent
            {
                f_uid = Guid.NewGuid(),
                f_create_date = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid(),
                ShippingManifestId = id,
                NewStatus = request.NewStatus,
                Location = request.StatusLocation,
                Comment = request.Comment
            });

            return NoContent();
        }
    }
}