using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using SharedLibraryAgents.Models;
using SharedLibraryAgents.Interfaces;

namespace PatchServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentMonitoredProductsController : ControllerBase
    {
        private readonly IAgentMonitoredProductsRepository _repository;

        public AgentMonitoredProductsController(IAgentMonitoredProductsRepository repository)
        {
            _repository = repository;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<AgentMonitoredProducts>>> GetAll([FromQuery] string? agentId = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(agentId))
                {
                    // This is what my service needs: api/AgentMonitoredProducts?agentId=XXX
                    var agentProducts = await _repository.GetByAgentIdAsync(agentId);
                    return Ok(agentProducts);
                }

                // Return all products for admin purposes
                var allProducts = await _repository.GetAllAsync();
                return Ok(allProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpGet("agent/{agentId}/product/{productName}")]
        public async Task<ActionResult<AgentMonitoredProducts>> GetByAgentIdAndProduct(string agentId, string productName)
        {
            var product = await _repository.GetByAgentIdAndProductAsync(agentId, productName);

            if (product == null)
                return NotFound($"No product named '{productName}' found for agent '{agentId}'");

            return Ok(product);
        }



        [HttpPost]
        public async Task<IActionResult> Add(AgentMonitoredProducts product)
        {
            await _repository.AddAsync(product);
            return Ok();
        }


        [HttpDelete("{agentId}/{product}")]
        public async Task<IActionResult> Delete(string agentId, string product)
        {
            var existing = await _repository.GetByAgentIdAndProductAsync(agentId, product);
            if (existing == null)
                return NotFound();

            await _repository.DeleteAsync(existing.Id); // Still needs Id internally
            return Ok();
        }

        // needed for updating product versions internally by my agent
        [HttpPut("updateVersion")]
        public async Task<IActionResult> UpdateProductVersion( UpdateProductVersionRequest request)
        {
            try
            {
                var existingProduct = await _repository.GetByAgentIdAndProductAsync(request.AgentId, request.MonitoredProduct);
                if (existingProduct == null)
                    return NotFound($"Product {request.MonitoredProduct} not found for agent {request.AgentId}");

                existingProduct.CurrentVersion = request.CurrentVersion;
                await _repository.UpdateAsync(existingProduct);
                return Ok(new { message = "Product version updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
