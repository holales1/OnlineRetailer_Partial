using CustomerApi.Models;
using System.Collections.Generic;

namespace CustomerApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
        void Edit(T entity);
        void Remove(int id);
        Customer Get(int? customerId);
    }
}
