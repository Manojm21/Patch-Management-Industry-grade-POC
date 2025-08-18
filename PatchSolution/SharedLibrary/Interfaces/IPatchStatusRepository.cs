using SharedLibrary.Models;

namespace SharedLibrary.Interfaces
{
    public interface IPatchStatusRepository
    {
        Task AddStatusReportAsync(PatchStatusReport status);
        Task<List<PatchStatusReport>> GetAllReportsAsync();
    }
}

