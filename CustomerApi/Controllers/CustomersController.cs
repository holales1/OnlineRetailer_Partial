using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Data;
using CustomerApi.Models;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> repository;

        public CustomersController(IRepository<Customer> repos)
        {
            repository = repos;
        }

        // GET: customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return repository.GetAll();
        }

        // GET customers/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST customers
        [HttpPost]
        public IActionResult Post([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            var newCustomer = repository.Add(customer);

            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, newCustomer);
        }

        // PUT customers/3
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Customer customer)
        {
            if (customer == null || customer.Id != id)
            {
                return BadRequest();
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Phone = customer.Phone;
            modifiedCustomer.ShippingAddress = customer.ShippingAddress;
            modifiedCustomer.BillingAddress = customer.BillingAddress;
            modifiedCustomer.Email = customer.Email;

            repository.Edit(modifiedCustomer);
            return new NoContentResult();
        }


    }
}
