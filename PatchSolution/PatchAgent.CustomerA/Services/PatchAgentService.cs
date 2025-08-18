using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibraryAgents.Enums;
using SharedLibraryAgents.Interfaces;
using SharedLibraryAgents.Models;
using PatchAgent.CustomerA.Utilities;

namespace PatchAgent.CustomerA.Services
{
    public class PatchAgentService : IPatchAgentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PatchAgentService> _logger;
        private readonly PatchPathSettings _pathSettings;
        private readonly string _agentId;

        // Utilities
        private readonly ApiUtility _apiUtility;
        private readonly VersionUtility _versionUtility;
        private readonly FileUtility _fileUtility;
        private readonly LoggingUtility _loggingUtility;
        private readonly CleanupUtility _cleanupUtility;

        public PatchAgentService(
            IConfiguration configuration,
            ILogger<PatchAgentService> logger,
            IOptions<PatchPathSettings> pathOptions,
            ApiUtility apiUtility,
            VersionUtility versionUtility,
            FileUtility fileUtility,
            LoggingUtility loggingUtility,
            CleanupUtility cleanupUtility)
        {
            _configuration = configuration;
            _logger = logger;
            _pathSettings = pathOptions.Value;
            _agentId = _configuration["AgentSettings:AgentId"] ?? "DefaultAgent";

            _apiUtility = apiUtility;
            _versionUtility = versionUtility;
            _fileUtility = fileUtility;
            _loggingUtility = loggingUtility;
            _cleanupUtility = cleanupUtility;
        }

        public async Task CheckAndApplyPatchAsync(CancellationToken cancellationToken = default)
        {
            // First, check and update agent version if needed
            await CheckAndUpdateAgentVersionAsync();

            var products = await _apiUtility.GetMonitoredProductsAsync(_agentId);

            _logger.LogInformation("Starting patch check for {ProductCount} products", products.Count);
            await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting patch check for {products.Count} products");

            foreach (var product in products)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _logger.LogInformation("Checking updates for product: {Product}", product.MonitoredProduct);
                    await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Checking updates for product: {product.MonitoredProduct}");

                    // Verify local version against database version
                    await VerifyAndSyncProductVersionAsync(product);

                    if (await IsUpdateAvailableAsync(_agentId, product.MonitoredProduct))
                    {
                        var latestPatch = await _apiUtility.GetLatestPatchAsync(_agentId, product.MonitoredProduct);
                        if (latestPatch != null)
                        {
                            await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] New patch available for {product.MonitoredProduct}: {latestPatch.Version}");
                            await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Downloading and applying patch...");

                            await DownloadAndApplyPatchAsync(latestPatch, product.MonitoredProduct);
                            await SaveCurrentPatchVersionAsync(product.AgentId,product.MonitoredProduct, latestPatch.Version);

                            // Update database with new product version
                            await _apiUtility.UpdateProductVersionAsync(_agentId, product.MonitoredProduct, latestPatch.Version);

                            await _apiUtility.ReportPatchStatusAsync(new PatchStatusDto
                            {
                                AgentId = _agentId,
                                PatchVersion = latestPatch.Version,
                                ProductName = product.MonitoredProduct,
                                TargetType = latestPatch.TargetType,
                                Status = PatchStatusEnum.Applied,
                                Timestamp = DateTime.UtcNow
                            });

                            _logger.LogInformation("Successfully applied patch {Version} for {Product}", latestPatch.Version, product.MonitoredProduct);
                            await _loggingUtility.LogSuccessAsync($"Applied patch {latestPatch.Version} for {product.MonitoredProduct}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No updates available for product: {Product}", product.MonitoredProduct);
                        await _loggingUtility.LogSuccessAsync($"{product.MonitoredProduct} is up to date");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update product: {Product}", product.MonitoredProduct);
                    await _loggingUtility.LogErrorAsync(ex, $"Error updating {product.MonitoredProduct}");

                    var latestPatch = await _apiUtility.GetLatestPatchAsync(_agentId, product.MonitoredProduct);
                    var version = latestPatch?.Version ?? "x.x.x";

                    await _apiUtility.ReportPatchStatusAsync(new PatchStatusDto
                    {
                        AgentId = _agentId,
                        PatchVersion =version ,
                        ProductName = product.MonitoredProduct,
                        TargetType = PatchTargetTypeEnum.Product,
                        Status = PatchStatusEnum.Failed,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Patch check completed for all products");
            await _loggingUtility.LogSeparatorAsync();
        }

        private async Task CheckAndUpdateAgentVersionAsync()
        {
            //try
            //{
            //    _logger.LogInformation("Checking agent version for {AgentId}", _agentId);
            //    await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Checking agent version...");

            //    // Check if agent patch is available
            //    var latestAgentPatch = await _apiUtility.GetLatestPatchAsync(_agentId, "Agent");
            try
            {
                _logger.LogInformation("Checking agent version for {AgentId}", _agentId);
                await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Checking agent version...");

                // DEBUG: Add this log
                _logger.LogInformation("DEBUG: Calling GetLatestPatchAsync for Agent");

                var latestAgentPatch = await _apiUtility.GetLatestPatchAsync(_agentId, "Agent");

                // DEBUG: Add this log
                _logger.LogInformation("DEBUG: Latest agent patch response: {Patch}",
                    latestAgentPatch != null ? $"Version: {latestAgentPatch.Version}, TargetType: {latestAgentPatch.TargetType}" : "null");

                if (latestAgentPatch != null && latestAgentPatch.TargetType == PatchTargetTypeEnum.Agent)
                {
                    var currentAgent = await _apiUtility.GetCustomerAgentAsync(_agentId);
                    var currentAgentVersion = currentAgent?.CurrentVersion ?? "0.0.0";
                    var localAgentVersion = await GetCurrentPatchVersionAsync(_agentId,"Agent");

                    var dbVersion = _versionUtility.CleanAndParseVersion(currentAgentVersion);
                    var latestVersion = _versionUtility.CleanAndParseVersion(latestAgentPatch.Version);

                    if (latestVersion != null && dbVersion != null && latestVersion > dbVersion)
                    {
                        await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] New agent version available: {currentAgentVersion} → {latestAgentPatch.Version}");

                        await DownloadAndApplyPatchAsync(latestAgentPatch, "Agent");
                        await SaveCurrentPatchVersionAsync(_agentId,"Agent", latestAgentPatch.Version);

                        // Update agent version in database
                        await _apiUtility.UpdateAgentVersionAsync(_agentId, latestAgentPatch.Version);

                        await _apiUtility.ReportPatchStatusAsync(new PatchStatusDto
                        {
                            AgentId = _agentId,
                            PatchVersion = latestAgentPatch.Version,
                            ProductName = "Agent",
                            TargetType = PatchTargetTypeEnum.Agent,
                            Status = PatchStatusEnum.Applied,
                            Timestamp = DateTime.UtcNow
                        });

                        await _loggingUtility.LogSuccessAsync($"Agent updated to version {latestAgentPatch.Version}");
                    }
                    else
                    {
                        _logger.LogInformation("DEBUG: No agent patch available or wrong target type");
                        await _loggingUtility.LogSuccessAsync("Agent is up to date");
                        //await _loggingUtility.LogSuccessAsync("Agent is up to date");
                    }
                }
            }
            catch (Exception ex)
            {

                var latestAgentPatch = await _apiUtility.GetLatestPatchAsync(_agentId, "Agent");
                var version = latestAgentPatch?.Version ?? "x.x.x";

                await _apiUtility.ReportPatchStatusAsync(new PatchStatusDto
                {
                    AgentId = _agentId,
                    PatchVersion = version,
                    ProductName = "Agent",
                    TargetType = PatchTargetTypeEnum.Agent,
                    Status = PatchStatusEnum.Failed,
                    Timestamp = DateTime.UtcNow
                });
                _logger.LogError(ex, "Error checking agent version");
                await _loggingUtility.LogErrorAsync(ex, "Error checking agent version");
            }
        }

        private async Task VerifyAndSyncProductVersionAsync(AgentMonitoredProducts product)
        {
            try
            {
                var localVersion = await GetCurrentPatchVersionAsync(product.AgentId,product.MonitoredProduct);
                var dbVersion = product.CurrentVersion ?? "0.0.0";

                if (localVersion != dbVersion)
                {
                    _logger.LogWarning("Version mismatch for {Product}. Local: {Local}, DB: {Database}",
                        product.MonitoredProduct, localVersion, dbVersion);
                    await _loggingUtility.LogWarningAsync($"Version mismatch for {product.MonitoredProduct}. Local: {localVersion}, DB: {dbVersion}");

                    // Use the higher version as the current version
                    var currentVersion = _versionUtility.GetHigherVersion(localVersion, dbVersion);

                    // Update both local file and database to match
                    await SaveCurrentPatchVersionAsync(product.AgentId, product.MonitoredProduct, currentVersion);
                    await _apiUtility.UpdateProductVersionAsync(_agentId, product.MonitoredProduct, currentVersion);

                    await _loggingUtility.LogSuccessAsync($"Synced {product.MonitoredProduct} version to {currentVersion}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying product version for {Product}", product.MonitoredProduct);
                await _loggingUtility.LogWarningAsync($"Error verifying version for {product.MonitoredProduct}");
            }
        }

        public async Task<bool> IsUpdateAvailableAsync(string agentId, string productName)
        {
            try
            {
                var latestPatch = await _apiUtility.GetLatestPatchAsync(agentId, productName);
                if (latestPatch == null) return false;

                var currentVersionString = await GetCurrentPatchVersionAsync(agentId,productName);
                var latestVersionString = latestPatch.Version;

                if (!_versionUtility.IsUpdateAvailable(currentVersionString, latestVersionString))
                {
                    return false;
                }

                await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Update available for {productName}: {currentVersionString} → {latestVersionString}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates for {Product}", productName);
                await _loggingUtility.LogErrorAsync(ex, $"Error checking updates for {productName}");
                return false;
            }
        }

        public async Task<PatchDto?> GetLatestPatchAsync(string agentId, string productName)
        {
            return await _apiUtility.GetLatestPatchAsync(agentId, productName);
        }

        public async Task DownloadAndApplyPatchAsync(PatchDto patch, string productName)
        {
            try
            {
                _logger.LogInformation("Applying patch {Version} for {Product}", patch.Version, productName);
                await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Downloading patch {patch.Version} for {productName}...");

                var productBasePath = Path.Combine(_pathSettings.PatchDownloadBasePath, _agentId, productName);
                await _cleanupUtility.CleanupOldPatchesAsync(productBasePath, productName, keepLatestCount: 2);

                var productPath = Path.Combine(_pathSettings.PatchDownloadBasePath, _agentId, productName, patch.Version);
                Directory.CreateDirectory(productPath);

                var filePath = Path.Combine(productPath, "patch.zip");
                var bytes = await _apiUtility.DownloadPatchAsync(patch.DownloadUrl);

                if (bytes == null)
                    throw new InvalidOperationException("Failed to download patch data");

                await File.WriteAllBytesAsync(filePath, bytes);

                var extractPath = Path.Combine(productPath, "extracted");
                await _fileUtility.ExtractPatchAsync(filePath, extractPath);

                await _fileUtility.CreateInstallationInstructionsAsync(productPath, productName, patch.Version, extractPath);

                await ShowPatchNotificationAsync(productName, patch.Version, productPath);

                await _loggingUtility.LogSuccessAsync($"Patch {patch.Version} ready for installation at: {productPath}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply patch {Version} for {Product}", patch.Version, productName);
                await _loggingUtility.LogErrorAsync(ex, $"Failed to download/apply patch {patch.Version} for {productName}");
                throw;
            }
        }

        private async Task ShowPatchNotificationAsync(string productName, string version, string patchPath)
        {
            try
            {
                await _fileUtility.CreateDesktopNotificationAsync(productName, version, patchPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to show patch notification");
                await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 🔔 IMPORTANT: New patch {version} for {productName} is ready!");
                await _loggingUtility.LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 📁 Location: {patchPath}");
            }
        }

        public async Task ReportPatchStatusAsync(PatchStatusDto status)
        {
            await _apiUtility.ReportPatchStatusAsync(status);
        }

        public async Task SaveCurrentPatchVersionAsync(string agentId, string productName, string version)
        {
            var versionDirectory = Path.Combine(_pathSettings.PatchDownloadBasePath, agentId, "localcheck");
            Directory.CreateDirectory(versionDirectory); // Ensure the directory exists

            var versionFile = Path.Combine(versionDirectory, $"{productName}.txt");
            await _fileUtility.SaveVersionFileAsync(versionFile, version);
        }


        public async Task<string> GetCurrentPatchVersionAsync(string agentId, string productName)
        {
            var versionFile = Path.Combine(_pathSettings.PatchDownloadBasePath, agentId, "localcheck", $"{productName}.txt");
            return await _fileUtility.ReadVersionFileAsync(versionFile);
        }

    }
}