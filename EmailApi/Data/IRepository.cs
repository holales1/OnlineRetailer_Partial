using System.Collections.Generic;

namespace EmailApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
    }
}
