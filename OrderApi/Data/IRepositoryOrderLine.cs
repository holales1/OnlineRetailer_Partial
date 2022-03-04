using OrderApi.Models;
using System.Collections.Generic;

namespace OrderApi.Data
{
    public interface IRepositoryOrderLine<T>
    {
        IEnumerable<T> GetByOrderId(int orderId);
        T Add(T entity);
    }
}
