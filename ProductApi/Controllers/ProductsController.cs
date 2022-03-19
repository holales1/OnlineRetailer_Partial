using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using System.Collections.Generic;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> repository;
        private readonly IConverter<Product, ProductDto> productConverter;

        public ProductsController(IRepository<Product> repos, IConverter<Product, ProductDto> converter)
        {
            repository = repos;
            productConverter = converter;
        }

        // GET products
        [HttpGet]
        public IEnumerable<ProductDto> Get()
        {
            return productConverter.ConvertList(repository.GetAll());
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            ProductDto productDto = productConverter.Convert(item);
            return new ObjectResult(productDto);
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody] ProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest();
            }

            Product product = productConverter.Convert(productDto);
            product = repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, productConverter.Convert(product));
        }

        // PUT products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] ProductDto productDto)
        {
            if (productDto == null || productDto.Id != id)
            {
                return BadRequest();
            }

            Product modifiedProduct = repository.Get(id);

            if (modifiedProduct == null)
            {
                return NotFound();
            }

            modifiedProduct.Name = productDto.Name;
            modifiedProduct.Price = productDto.Price;
            modifiedProduct.Category = productDto.Category;
            modifiedProduct.ItemsInStock = productDto.ItemsInStock;
            modifiedProduct.ItemsReserved = productDto.ItemsReserved;

            repository.Edit(modifiedProduct);
            return new NoContentResult();
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }

    }
}
