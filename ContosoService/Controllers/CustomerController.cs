using Microsoft.AspNetCore.Mvc;
using Contoso.Models;
using Microsoft.AspNetCore.Authorization;

namespace Contoso.Service.Controllers
{
    namespace Contoso.Service.Controllers
    {
        /// <summary>
        /// Contains methods for interacting with customer data.
        /// </summary>
        [ApiController]
        [Authorize(Policy = "AuthZPolicy")]
        [Route("api/[controller]")]
        public class CustomerController : ControllerBase
        {
            private ICustomerRepository _repository; 

            public CustomerController(ICustomerRepository repository)
            {
                _repository = repository; 
            }

            /// <summary>
            /// Gets all customers. 
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> Get()
            {
                return Ok(await _repository.GetAsync()); 
            }

            /// <summary>
            /// Gets the customer with the given id.
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> Get(Guid id)
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(); 
                }
                var customer = await _repository.GetAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(customer); 
                }
            }

            /// <summary>
            /// Gets all customers with a data field matching the start of the given string.
            /// </summary>
            [HttpGet("search")]
            public async Task<IActionResult> Search(string value)
            {
                return Ok(await _repository.GetAsync(value)); 
            }

            /// <summary>
            /// Adds a new customer or updates an existing one.
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Post([FromBody]Customer customer)
            {
                return Ok(await _repository.UpsertAsync(customer)); 
            }

            /// <summary>
            /// Deletes a customer and all data associated with them.
            /// </summary>
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(Guid id)
            {
                await _repository.DeleteAsync(id);
                return Ok(); 
            }
        }
    }
}
