using System.Collections.Generic;

namespace ProductApi.Models
{
    public interface IConverter<T, U>
    {
        T Convert(U model);
        U Convert(T model);
        IEnumerable<T> ConvertList(IEnumerable<U> modelList);
        IEnumerable<U> ConvertList(IEnumerable<T> modelList);
    }
}
