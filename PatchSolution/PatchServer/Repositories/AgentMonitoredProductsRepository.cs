using Microsoft.EntityFrameworkCore;
using PatchServer.Data;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using SharedLibraryAgents.Interfaces;
using SharedLibraryAgents.Models;

namespace PatchServer.Repositories
{
    public class AgentMonitoredProductsRepository : IAgentMonitoredProductsRepository
    {
        private readonly PatchDbContext _context;

        public AgentMonitoredProductsRepository(PatchDbContext context)
        {
            _context = context;
        }

        public async Task<List<AgentMonitoredProducts>> GetAllAsync()
        {
            return await _context.AgentMonitoredProducts.ToListAsync();
        }

        public async Task<AgentMonitoredProducts?> GetByIdAsync(int id)
        {
            return await _context.AgentMonitoredProducts.FindAsync(id);
        }

        public async Task<List<AgentMonitoredProducts>> GetByAgentIdAsync(string agentId)
        {
            return await _context.AgentMonitoredProducts
                .Where(p => p.AgentId == agentId)
                .ToListAsync();
        }

        public async Task<AgentMonitoredProducts?> GetByAgentIdAndProductAsync(string agentId, string productName)
        {
            return await _context.AgentMonitoredProducts
                .FirstOrDefaultAsync(p => p.AgentId == agentId && p.MonitoredProduct == productName);
        }

        public async Task AddAsync(AgentMonitoredProducts product)
        {
            await _context.AgentMonitoredProducts.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AgentMonitoredProducts product)
        {
            _context.AgentMonitoredProducts.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.AgentMonitoredProducts.FindAsync(id);
            if (product != null)
            {
                _context.AgentMonitoredProducts.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
