using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace PatchAgent.CustomerA.Utilities
{
    //all my file works like notification and logging here
    public class FileUtility
    {
        private readonly ILogger<FileUtility> _logger;

        public FileUtility(ILogger<FileUtility> logger)
        {
            _logger = logger;
        }

        public async Task<string> ReadVersionFileAsync(string versionFilePath)
        {
            try
            {
                if (!File.Exists(versionFilePath))
                    return "0.0.0";

                return (await File.ReadAllTextAsync(versionFilePath)).Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read version file: {FilePath}", versionFilePath);
                return "0.0.0";
            }
        }

        public async Task SaveVersionFileAsync(string versionFilePath, string version)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(versionFilePath)!);
                await File.WriteAllTextAsync(versionFilePath, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save version file: {FilePath}", versionFilePath);
                throw;
            }
        }

        public async Task ExtractPatchAsync(string zipFilePath, string extractPath)
        {
            try
            {
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, recursive: true);
                }
                Directory.CreateDirectory(extractPath);

                ZipFile.ExtractToDirectory(zipFilePath, extractPath, overwriteFiles: true);
                _logger.LogInformation("Extracted patch to: {ExtractPath}", extractPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract patch: {ZipPath}", zipFilePath);
                throw;
            }
        }

        public async Task CreateInstallationInstructionsAsync(string patchPath, string productName, string version, string extractPath)
        {
            var instructionsPath = Path.Combine(patchPath, "INSTALL_INSTRUCTIONS.txt");
            var instructions = $@"
==================================================
PATCH INSTALLATION INSTRUCTIONS
==================================================

Product: {productName}
Version: {version}
Downloaded: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

STEPS TO INSTALL:
1. Close {productName} application if running
2. Navigate to: {extractPath}
3. Copy all files from extracted folder to your {productName} installation directory
4. Restart {productName}

EXTRACTED FILES LOCATION:
{extractPath}

BACKUP RECOMMENDATION:
Before installing, backup your current {productName} files.

==================================================
Need help? Check the patch log file on your Desktop.
==================================================
";

            await File.WriteAllTextAsync(instructionsPath, instructions);
        }

        public async Task CreateDesktopNotificationAsync(string productName, string version, string patchPath)
        {
            try
            {
                //var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PatchAgent", "installnotifications");
                //Directory.CreateDirectory(desktopPath); 
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var notificationFile = Path.Combine(desktopPath, $"🔔PATCH_READY_{productName}_{version}.txt");

                var content = $@"
🚨🚨🚨 URGENT: PATCH READY FOR INSTALLATION 🚨🚨🚨
════════════════════════════════════════════════════════════

🔔 NOTIFICATION: New patch available and ready!
📦 Product: {productName}
🆚 Version: {version}
⏰ Ready Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
📁 Patch Location: {patchPath}

🎯 WHAT TO DO NEXT:
════════════════════════════════════════════════════════════
1️⃣ Close {productName} application (if running)
2️⃣ Open the patch folder: {patchPath}
3️⃣ Read the INSTALL_INSTRUCTIONS.txt file
4️⃣ Copy files from 'extracted' folder to your {productName} installation
5️⃣ Restart {productName}
6️⃣ Delete this notification file when done

⚠️ IMPORTANT NOTES:
════════════════════════════════════════════════════════════
• Backup your current {productName} files before installing
• Installation files are in the 'extracted' subfolder
• Check daily patch logs in Desktop/PatchAgent folder for details

🔗 Quick Links:
════════════════════════════════════════════════════════════
• Patch Folder: {patchPath}
• Log Files: {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\PatchAgent

🚨🚨🚨 ACTION REQUIRED - PLEASE INSTALL PATCH 🚨🚨🚨
";

                await File.WriteAllTextAsync(notificationFile, content);
                _logger.LogInformation("Created prominent desktop notification: {NotificationFile}", notificationFile);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create desktop notification file");
                throw;
            }
        }
    }
}