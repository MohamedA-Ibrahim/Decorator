using Contoso.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contoso.Service.Controllers
{
    /// <summary>
    /// Contains methods for interacting with product data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all products in the database.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _repository.GetAsync()); 
        }

        /// <summary>
        /// Gets the product with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(); 
            }
            var products = await _repository.GetAsync(id); 
            if (products == null)
            {
                return NotFound(); 
            }
            return Ok(products); 
        }


        /// <summary>
        /// Gets all products with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return BadRequest();
            }
            var products = await _repository.GetAsync(value);
            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }
    }
}
