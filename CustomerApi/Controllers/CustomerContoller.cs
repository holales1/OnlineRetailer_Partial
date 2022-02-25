using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Data;
using CustomerApi.Models;
using RestSharp;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerContoller : ControllerBase
    {
        private readonly IRepository<Customer> repository;

        public CustomerContoller(IRepository<Customer> repos)
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
        [HttpGet("{id}", Name="GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST customer
        [HttpPost]
        public IActionResult Post([FromBody]Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            var newCustomer = repository.Add(customer);
           
            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, newCustomer );
        }

        //CREAR API PUT PARA CAMBIAR DATOS DEL CLIENTE
        //PUEDE CAMBIAR EMAIL, PHONE, BILLING ADDRESS & SHIPPING ADDRESS
        //Nuevo Comentario
        

    }
}
