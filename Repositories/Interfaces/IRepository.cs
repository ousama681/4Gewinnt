namespace VierGewinnt.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // CRUD Funktionen der Datenbank

        Task<bool> AddAsync(T item);

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(T item);

        //Task<List<string>> GetFilteredAsync(List<string> item);


        Task UpdateAsync(T item);

        Task<bool> DeleteAsync(T item);
    }
}
