using Microsoft.Extensions.Logging;
using SharedLibraryAgents.Enums;
using SharedLibraryAgents.Models;
using System.Net.Http.Json;

namespace PatchAgent.CustomerA.Utilities
{
    //all the utilities are listed here for API comm
    public class ApiUtility
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiUtility> _logger;
        private readonly LoggingUtility _loggingUtility;

        public ApiUtility(
            HttpClient httpClient,
            ILogger<ApiUtility> logger,
            LoggingUtility loggingUtility)
        {
            _httpClient = httpClient;
            _logger = logger;
            _loggingUtility = loggingUtility;
        }

        public async Task<List<AgentMonitoredProducts>> GetMonitoredProductsAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AgentMonitoredProducts>>(
                    $"AgentMonitoredProducts?agentId={agentId}");

                return response ?? new List<AgentMonitoredProducts>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch monitored products for agent {AgentId}", agentId);
                return new List<AgentMonitoredProducts>();
            }
        }

        public async Task<PatchDto?> GetLatestPatchAsync(string agentId, string productName)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PatchDto>($"patch/latest?agentId={agentId}&product={productName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get latest patch for {Product}", productName);
                await _loggingUtility.LogErrorAsync(ex, $"Failed to check for latest patch for {productName}");
                return null;
            }
        }

        public async Task<CustomerAgent?> GetCustomerAgentAsync(string agentId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CustomerAgent>($"CustomerAgent/{agentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get agent version from database for {AgentId}", agentId);
                return null;
            }
        }

        public async Task<bool> UpdateAgentVersionAsync(string agentId, string newVersion)
        {
            try
            {
                var updateRequest = new
                {
                    AgentId = agentId,
                    CurrentVersion = newVersion
                };

                var response = await _httpClient.PutAsJsonAsync($"CustomerAgent/updateVersion", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated agent version in database to {Version}", newVersion);
                    await _loggingUtility.LogSuccessAsync($"Updated agent version in database: {newVersion}");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update agent version in database. StatusCode: {StatusCode}", response.StatusCode);
                    await _loggingUtility.LogWarningAsync("Failed to update agent version in database");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent version in database");
                await _loggingUtility.LogErrorAsync(ex, "Error updating agent version in database");
                return false;
            }
        }

        public async Task<bool> UpdateProductVersionAsync(string agentId, string productName, string newVersion)
        {
            try
            {
                var updateRequest = new
                {
                    AgentId = agentId,
                    MonitoredProduct = productName,
                    CurrentVersion = newVersion
                };

                var response = await _httpClient.PutAsJsonAsync($"AgentMonitoredProducts/updateVersion", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated product version in database for {Product} to {Version}", productName, newVersion);
                    await _loggingUtility.LogSuccessAsync($"Updated {productName} version in database: {newVersion}");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update product version in database for {Product}. StatusCode: {StatusCode}", productName, response.StatusCode);
                    await _loggingUtility.LogWarningAsync($"Failed to update {productName} version in database");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product version in database for {Product}", productName);
                await _loggingUtility.LogErrorAsync(ex, $"Error updating {productName} version in database");
                return false;
            }
        }

        public async Task<bool> ReportPatchStatusAsync(PatchStatusDto status)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("status/report", status);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Status reported successfully for {Status}", status.Status);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to report status. StatusCode: {StatusCode}", response.StatusCode);
                    await _loggingUtility.LogWarningAsync("Failed to report status to server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting patch status");
                await _loggingUtility.LogWarningAsync("Error reporting patch status to server");
                return false;
            }
        }

        public async Task<byte[]?> DownloadPatchAsync(string downloadUrl)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(downloadUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download patch from {Url}", downloadUrl);
                return null;
            }
        }
    }
}