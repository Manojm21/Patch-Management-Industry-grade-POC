using Microsoft.EntityFrameworkCore;
using PatchServer.Data;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;

public class PatchRepository : IPatchRepository
{
    private readonly PatchDbContext _context;

    public PatchRepository(PatchDbContext context)
    {
        _context = context;
    }

    public async Task<List<Patch>> GetAllPatchesAsync()
        => await _context.Patches.ToListAsync();

    public async Task<Patch?> GetPatchByIdAsync(int id)
        => await _context.Patches.FindAsync(id);

    public async Task AddPatchAsync(Patch patch)
    {
        _context.Patches.Add(patch);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePatchAsync(Patch patch)
    {
        _context.Patches.Update(patch);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePatchAsync(int id)
    {
        var patch = await _context.Patches.FindAsync(id);
        if (patch != null)
        {
            _context.Patches.Remove(patch);
            await _context.SaveChangesAsync();
        }
    }
}
