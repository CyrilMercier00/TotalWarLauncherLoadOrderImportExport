using LauncherMiddleware.Models;

namespace LauncherMiddleware;

public class Import
{
    // Get current config
    // Extract mod list for x game from config
    //
    // Check for unsubscribed mods
    // Subscribe to missing mods 
    //
    // Build new config from import
    //
    // Add mods present in initial config but not in exported one

    public List<Mod> Get (List<Mod> importedMods, List<Mod> existingMods)
    {
        if (!importedMods.Any()) throw new ArgumentException(null, nameof (importedMods));

        var newModList = new List<Mod>();

        return newModList;
    }
}
