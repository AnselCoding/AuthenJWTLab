using AuthenJWTLab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuthenJWTLab.Repository
{
    // Version 2：可以多個 entity 通用此泛型 Repository
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _db;
        private readonly LabDBContext _context;

        public Repository(LabDBContext context)
        {
            _context = context;
            _db = context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            _db.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _db.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _db.ToListAsync();
        }

        public async Task<T> GetAsync(int id)
        {
            return await _db.FindAsync(id);
        }

        public bool IsItemExists(int id)
        {
            var pk = _db.EntityType.FindPrimaryKey()?.GetName();
            if (pk == null)
            {
                return false;
            }
            return (_db?.Any(e => (int)typeof(T).GetProperty(pk).GetValue(e) == id)).GetValueOrDefault();
        }

    }
}
