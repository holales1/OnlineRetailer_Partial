using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models
{
    public interface IConverter<T,U>
    {
        T Convert(U model);
        U Convert(T model);
        IEnumerable<T> ConvertList(IEnumerable<U> modelList);
        IEnumerable<U> ConvertList(IEnumerable<T> modelList);
    }
}
