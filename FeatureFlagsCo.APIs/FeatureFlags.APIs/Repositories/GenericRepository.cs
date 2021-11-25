using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Repositories
{
    public interface IGenericRepository
    {
        Task<List<T>> SelectListAsync<T>(int pageIndex = 0, int pageSize = 999999) where T : class;
        Task<T> SelectByIdAsync<T>(int id) where T : class;
        Task<T> CreateAsync<T>(T entity) where T : class;
        Task<T> UpdateAsync<T>(T entity) where T : class;
        Task DeleteAsync<T>(int id) where T : class;
    }

    public class GenericRepository<ApplicationDbContext> : IGenericRepository where ApplicationDbContext : DbContext
    {
        protected ApplicationDbContext dbContext;

        public GenericRepository(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<T> CreateAsync<T>(T entity) where T : class
        {
            var newEdntity = this.dbContext.Set<T>().Add(entity);
            await this.dbContext.SaveChangesAsync();
            return newEdntity.Entity;
        }

        public async Task DeleteAsync<T>(int id) where T : class
        {
            var entity = await this.dbContext.Set<T>().FindAsync(id);
            this.dbContext.Set<T>().Remove(entity);
            _ = await this.dbContext.SaveChangesAsync();
        }

        public async Task<List<T>> SelectListAsync<T>(int pageIndex = 0, int pageSize = 999999) where T : class
        {
            return await this.dbContext.Set<T>().Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        }


        public async Task<T> SelectByIdAsync<T>(int id) where T : class
        {
            return await this.dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T> UpdateAsync<T>(T entity) where T : class
        {
            var updatedEntity = this.dbContext.Set<T>().Update(entity);
            _ = await this.dbContext.SaveChangesAsync();
            return updatedEntity.Entity;
        }
    }

}
