
using SharedLibrary.Models;

namespace SharedLibrary.Interfaces
{
    public interface IPatchRepository
    {
        Task<List<Patch>> GetAllPatchesAsync();
        Task<Patch> GetPatchByIdAsync(int id);
        Task AddPatchAsync(Patch patch);
        Task UpdatePatchAsync(Patch patch);
        Task DeletePatchAsync(int id);
    }
}

