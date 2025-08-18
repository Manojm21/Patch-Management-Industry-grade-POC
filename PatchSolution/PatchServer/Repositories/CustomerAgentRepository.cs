using Microsoft.EntityFrameworkCore;
using PatchServer.Data;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using SharedLibraryAgents.Interfaces;
using SharedLibraryAgents.Models;

public class CustomerAgentRepository : ICustomerAgentRepository
{
    private readonly PatchDbContext _context;

    public CustomerAgentRepository(PatchDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerAgent>> GetAllAgentsAsync()
        => await _context.CustomerAgents.ToListAsync();

    public async Task<CustomerAgent?> GetAgentByIdAsync(int id)
        => await _context.CustomerAgents.FindAsync(id);

    public async Task<CustomerAgent?> GetByAgentIdAsync(string agentId)
            => await _context.CustomerAgents.FirstOrDefaultAsync(a => a.AgentId == agentId);
    public async Task AddAgentAsync(CustomerAgent agent)
    {
        _context.CustomerAgents.Add(agent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAgentAsync(CustomerAgent agent)
    {
        _context.CustomerAgents.Update(agent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAgentAsync(int id)
    {
        var agent = await _context.CustomerAgents.FindAsync(id);
        if (agent != null)
        {
            _context.CustomerAgents.Remove(agent);
            await _context.SaveChangesAsync();
        }
    }
}

