using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLibrary.Models;
using SharedLibraryAgents.Models;

namespace SharedLibrary.Interfaces
{
    public interface ICustomerAgentRepository
    {
        Task<List<CustomerAgent>> GetAllAgentsAsync();
        Task<CustomerAgent> GetAgentByIdAsync(int id);
        Task<CustomerAgent?> GetByAgentIdAsync(string agentId);
        Task AddAgentAsync(CustomerAgent agent);
        Task UpdateAgentAsync(CustomerAgent agent);
        Task DeleteAgentAsync(int id);
    }
}
