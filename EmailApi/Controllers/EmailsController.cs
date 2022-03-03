using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EmailApi.Data;
using EmailApi.Models;

namespace EmailApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IRepository<Email> repository;

        public EmailsController(IRepository<Email> repos)
        {
            repository = repos;
        }

        // GET: emails
        [HttpGet]
        public IEnumerable<Email> Get()
        {
            return repository.GetAll();
        }

        // GET: emails
        [HttpGet("total")]
        public int GetTotal()
        {
            ICollection<Email> col = (ICollection<Email>)repository.GetAll();
            return col.Count;
        }

        // GET emails/5
        [HttpGet("{id}", Name = "GetEmail")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST emails
        [HttpPost]
        public IActionResult Post([FromBody] Email email)
        {
            if (email == null)
            {
                return BadRequest();
            }

            var newEmail = repository.Add(email);

            return CreatedAtRoute("GetEmail", new { id = newEmail.Id }, newEmail);
        }

    }
}
