using eShop.Contracts.Events;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.Models;
using eShop.CustomerService.Models.DTOs;
using eShop.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace eShop.CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IEventBus _eventBus;

        public CustomersController(ICustomerService customerService, IEventBus eventBus)
        {
            _customerService = customerService;
            _eventBus = eventBus;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();

            return Ok(customers.Select(profile => new
            {
                profile.f_uid,
                profile.f_email,
                profile.f_fname,
                profile.f_lname,
                profile.f_phone,
                profile.f_create_date
            }));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _customerService.GetProfileAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                profile.f_uid,
                profile.f_email,
                profile.f_fname,
                profile.f_lname,
                profile.f_phone,
                profile.f_create_date
            });
        }

        [HttpPost("onboard")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Onboard([FromBody] CustomerOnboardRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customer = new CustomerModel
                {
                    f_email = request.email,
                    f_password = request.password,
                    f_fname = request.firstName,
                    f_lname = request.lastName,
                    f_phone = request.phone
                };

                var address = new CustomerAddressModel
                {
                    f_street = request.street,
                    f_city = request.city,
                    f_state = request.state,
                    f_postal_code = request.postalCode
                };

                var success = await _customerService.OnboardNewCustomerAsync(customer, address);
                if (!success)
                {
                    return BadRequest(new { error = "Onboarding execution failed." });
                }

                await _eventBus.PublishAsync(new CustomerRegisteredEvent
                {
                    f_uid = Guid.NewGuid(),
                    f_create_date = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    CustomerId = customer.f_uid,
                    Email = customer.f_email,
                    FirstName = customer.f_fname,
                    LastName = customer.f_lname
                });

                return CreatedAtAction(nameof(GetById), new { id = customer.f_uid }, new
                {
                    customerUid = customer.f_uid,
                    message = "Customer onboarded successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _customerService.ModifyContactDetailsAsync(
                id,
                request.firstName,
                request.lastName,
                request.phone);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}