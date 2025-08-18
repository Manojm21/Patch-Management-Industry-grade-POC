using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NuGet.Versioning;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;
using SharedLibraryAgents.Models;


namespace PatchServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatchController : ControllerBase
    {
        private readonly IPatchRepository _repository;
        private readonly IConfiguration _configuration;

        public PatchController(IPatchRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patch>>> GetAll()
        {
            var patches = await _repository.GetAllPatchesAsync();
            return Ok(patches);
        }

        [HttpGet("product/{productName}/patch/{patchName}")]
        public async Task<ActionResult<Patch>> GetByProductAndPatch(string productName, string patchName)
        {
            var patches = await _repository.GetAllPatchesAsync();

            var match = patches.FirstOrDefault(p =>
                p.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                p.PatchName.Equals(patchName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return NotFound($"Patch '{patchName}' for product '{productName}' not found.");

            return Ok(match);
        }

        [HttpDelete("product/{productName}/patch/{patchName}")]
        public async Task<IActionResult> DeleteByProductAndPatch(string productName, string patchName)
        {
            var patches = await _repository.GetAllPatchesAsync();

            var match = patches.FirstOrDefault(p =>
                p.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                p.PatchName.Equals(patchName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return NotFound($"Patch '{patchName}' for product '{productName}' not found.");

            await _repository.DeletePatchAsync(match.Id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Patch patch)
        {
            await _repository.AddPatchAsync(patch);
            return Ok(new { message = "Patch created successfully", patch.ProductName, patch.PatchName });
        }


        [HttpPost("uploadZip")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadZip([FromForm] PatchUploadDto model)
        {
            try
            {
                // Validate input
                if (model.PatchZip == null || model.PatchZip.Length == 0)
                    return BadRequest("No file uploaded or file is empty");

                if (string.IsNullOrWhiteSpace(model.ProductName))
                    return BadRequest("ProductName is required");

                if (string.IsNullOrWhiteSpace(model.PatchName))
                    return BadRequest("PatchName is required");

                // Validate file extension
                var fileExtension = Path.GetExtension(model.PatchZip.FileName).ToLowerInvariant();
                if (fileExtension != ".zip")
                    return BadRequest("Only .zip files are allowed");

                // Check file size
                const long maxFileSize = 100 * 1024 * 1024; // 100MB
                if (model.PatchZip.Length > maxFileSize)
                    return BadRequest($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");

                var basePath = @"C:\PatchServer\patches";
                var targetDir = Path.Combine(basePath, model.ProductName, model.PatchName);

                // Create directory if it doesn't exist
                try
                {
                    Directory.CreateDirectory(targetDir);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Failed to create directory: {ex.Message}");
                }

                var filePath = Path.Combine(targetDir, "installer.zip");

                // Save the file
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PatchZip.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Failed to save file: {ex.Message}");
                }

                return Ok(new
                {
                    message = "Zip file uploaded successfully",
                    filePath = filePath,
                    fileSize = model.PatchZip.Length
                });
            }
            catch (Exception ex)
            {
                

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        //  Return latest patch based on agentId & product- called internally
        [HttpGet("latest")]
        public async Task<ActionResult<PatchDto>> GetLatestPatch([FromQuery] string agentId, [FromQuery] string product)
        {
            var patches = await _repository.GetAllPatchesAsync();

            var filtered = patches
    .Where(p => p.ProductName.Equals(product, StringComparison.OrdinalIgnoreCase)) 
    .OrderByDescending(p => NuGet.Versioning.NuGetVersion.Parse(p.Version))  
    .FirstOrDefault();


            if (filtered == null)
                return NoContent();
            // As of now we have to replace by static file location -downloadUrl present in DB.. to be done if i do cloud solution
            var baseDownloadUrl = _configuration["PatchServer:BaseDownloadUrl"] ?? "https://localhost:7253/patches";

            return Ok(new PatchDto
            {
                Version = filtered.Version,
                Description = filtered.Description,
                DownloadUrl = $"{baseDownloadUrl}/{product}/{filtered.PatchName}/installer.zip",
                TargetType = filtered.TargetType
            });
        }

        public class PatchUploadDto
        {
            [FromForm]
            public IFormFile PatchZip { get; set; }

            [FromForm]
            public string ProductName { get; set; }

            [FromForm]
            public string PatchName { get; set; }
        }

    }
}
