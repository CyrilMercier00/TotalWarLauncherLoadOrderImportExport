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


    public List<ModData> Get (List<ModData> importedMods, List<ModData> existingMods)
    {
        if (!importedMods.Any()) throw new ArgumentException(null, nameof (importedMods));

        var newModList = new List<ModData>();

        return newModList;
    }
}
