using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Repository
{
    public interface IReadSpecific<T> where T:class
    {
        T ReadSpecific(string identifyingItem);
    }
}
