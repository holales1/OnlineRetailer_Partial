using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EmailApi.Models;

namespace EmailApi.Data
{
    public class EmailRepository : IRepository<Email>
    {
        private readonly EmailApiContext db;

        public EmailRepository(EmailApiContext context)
        {
            db = context;
        }

        Email IRepository<Email>.Add(Email entity)
        {            
            var newEmail = db.Emails.Add(entity).Entity;
            db.SaveChanges();
            return newEmail;
        }

        void IRepository<Email>.Edit(Email entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Email IRepository<Email>.Get(int id)
        {
            return db.Emails.FirstOrDefault(o => o.Id == id);
        }

        IEnumerable<Email> IRepository<Email>.GetAll()
        {
            return db.Emails.ToList();
        }

        void IRepository<Email>.Remove(int id)
        {
            var email = db.Emails.FirstOrDefault(p => p.Id == id);
            db.Emails.Remove(email);
            db.SaveChanges();
        }
    }
}
