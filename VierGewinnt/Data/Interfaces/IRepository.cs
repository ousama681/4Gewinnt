namespace VierGewinnt.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {

        bool AddAsync(T item);

        List<T> GetAllAsync();

        T GetByIdAsync(T item);

        Task UpdateAsync(T item);

        bool DeleteAsync(T item);
    }
}
