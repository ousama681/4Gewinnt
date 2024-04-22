namespace VierGewinnt.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {

        Task<bool> AddAsync(T item);

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(T item);

        Task UpdateAsync(T item);

        Task<bool> DeleteAsync(T item);
    }
}
