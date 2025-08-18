using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using SharedLibraryAgents.Models;
using SharedLibraryAgents.Interfaces;

namespace PatchServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerAgentController : ControllerBase
    {
        private readonly ICustomerAgentRepository _repository;

        public CustomerAgentController(ICustomerAgentRepository repository)
        {
            _repository = repository;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<CustomerAgent>>> GetAll()
        {
            var agents = await _repository.GetAllAgentsAsync();
            return Ok(agents);
        }

        
        [HttpGet("{agentId}")]
        public async Task<ActionResult<CustomerAgent>> GetByAgentId(string agentId)
        {
            try
            {
                var agent = await _repository.GetByAgentIdAsync(agentId);
                if (agent == null)
                    return NotFound($"Agent with ID {agentId} not found");
                return Ok(agent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Add(CustomerAgent agent)
        {
            try
            {
                var existingAgent = await _repository.GetByAgentIdAsync(agent.AgentId);
                if (existingAgent != null)
                {
                    return Conflict(new { message = $"Agent with ID {agent.AgentId} already exists." });
                }

                await _repository.AddAgentAsync(agent);
                return Ok(new { message = "Agent added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }




        [HttpPut("updateVersion")]
        public async Task<IActionResult> UpdateAgentVersion(UpdateAgentVersionRequest request)
        {
            try
            {
                var existingAgent = await _repository.GetByAgentIdAsync(request.AgentId);
                if (existingAgent == null)
                    return NotFound($"Agent {request.AgentId} not found");

                existingAgent.CurrentVersion = request.CurrentVersion;
                await _repository.UpdateAgentAsync(existingAgent);

                return Ok(new { message = "Agent version updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpDelete("{agentId}")]
        public async Task<IActionResult> Delete(string agentId)
        {
            var existing = await _repository.GetByAgentIdAsync(agentId);
            if (existing == null)
                return NotFound($"Agent {agentId} not found");

            await _repository.DeleteAgentAsync(existing.Id);
            return Ok();
        }
    }
}
