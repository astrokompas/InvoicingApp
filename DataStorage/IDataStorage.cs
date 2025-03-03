using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace InvoicingApp.DataStorage
{
    public interface IDataStorage<T> where T : class, IEntity
    {
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task SaveAsync(T item);
        Task DeleteAsync(string id);
        Task<IEnumerable<T>> QueryAsync(Func<T, bool> predicate);
    }
}