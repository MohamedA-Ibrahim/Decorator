﻿using Contoso.Models;
using Microsoft.AspNetCore.Mvc;

namespace Contoso.Service.Controllers
{
    /// <summary>
    /// Contains methods for interacting with order data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repository; 

        public OrderController(IOrderRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _repository.GetAsync()); 
        }

        /// <summary>
        /// Gets the with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(); 
            }
            var order = await _repository.GetAsync(id); 
            if (order == null)
            {
                return NotFound(); 
            }
            return Ok(order); 
        }

        /// <summary>
        /// Gets all the orders for a given customer. 
        /// </summary>
        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetCustomerOrders(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(); 
            }
            var orders = await _repository.GetForCustomerAsync(id);
            if (orders == null)
            {
                return NotFound(); 
            }
            return Ok(orders); 
        }

        /// <summary>
        /// Gets all orders with a data field matching the start of the given string.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return BadRequest(); 
            }
            var orders = await _repository.GetAsync(value); 
            if (orders == null)
            {
                return NotFound(); 
            }
            return Ok(orders); 
        }


        /// <summary>
        /// Creates a new order or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Order order)
        {
            return Ok(await _repository.UpsertAsync(order)); 
        }

        /// <summary>
        /// Deletes an order.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Order order)
        {
            await _repository.DeleteAsync(order.Id);
            return Ok(); 
        }
    }
}
