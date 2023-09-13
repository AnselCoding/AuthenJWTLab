namespace AuthenJWTLab.Repository
{
    public interface IRepository<T> where T : class
    {
        bool IsItemExists(int id);
        Task CreateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task UpdateAsync(T entity);
    }
}