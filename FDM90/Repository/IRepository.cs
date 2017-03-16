using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Repository
{
    public interface IRepository<T> where T : class
    {
        void Create(T objectToCreate);
        IEnumerable<T> ReadAll();
        void Update(T objectToUpdate);
        void Delete(T objectId);
    }
}
