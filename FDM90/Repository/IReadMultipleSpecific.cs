using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Repository
{
    public interface IReadMultipleSpecific<T> where T : class
    {
        IEnumerable<T> ReadMultipleSpecific(string objectId);
    }
}
