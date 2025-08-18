using Microsoft.EntityFrameworkCore;
using PatchServer.Data;
using PatchServer.Repositories;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;

namespace PatchServer.Repositories
{

    public class PatchStatusRepository : IPatchStatusRepository
    {
        private readonly PatchDbContext _context;

        public PatchStatusRepository(PatchDbContext context)
        {
            _context = context;
        }

        public async Task AddStatusReportAsync(PatchStatusReport status)
        {
            _context.PatchStatusReports.Add(status);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PatchStatusReport>> GetAllReportsAsync()
            => await _context.PatchStatusReports.ToListAsync();
    }
}