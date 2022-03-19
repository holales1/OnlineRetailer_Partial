using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailApi.Models
{
    public interface IConverter<T,U>
    {
        T Convert(U model);
        U Convert(T model);
    }
}
