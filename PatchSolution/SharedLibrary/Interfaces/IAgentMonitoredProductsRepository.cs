using SharedLibraryAgents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IAgentMonitoredProductsRepository
    {
        Task<List<AgentMonitoredProducts>> GetAllAsync();
        Task<AgentMonitoredProducts?> GetByIdAsync(int id);
        Task<List<AgentMonitoredProducts>> GetByAgentIdAsync(string agentId);
        Task<AgentMonitoredProducts?> GetByAgentIdAndProductAsync(string agentId, string productName);
        Task AddAsync(AgentMonitoredProducts product);
        Task UpdateAsync(AgentMonitoredProducts product);
        Task DeleteAsync(int id);
    }

}
