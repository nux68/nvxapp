using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Interfaces
{
   

        public interface IRepository<T> where T : class
        {

            Task<List<T>> FindAll();
            IQueryable<T> FindAll(Expression<Func<T, bool>> where);
            T? FindById(int id);
            Task<T?> FindByIdAsync(int id);
            Task<T?> FindByIdAsync(int id1, int id2);
            Task<T?> FindByIdAsync(int id1, int id2, int id3);
            Task<T> UpsertAsync(T entity);
            Task<T> UpsertAsyncGuid(T entity);
            Task<T> CreateAsync(T entity, Boolean SaveChanges = true);
            Task<T> UpdateAsync(T entity);
            Task DeleteAsync(T entity);
            Task DeleteRangeAsync(IEnumerable<T> entities);
            DbSet<T> GetAll();

            Task SaveChange();

        }

    
}
