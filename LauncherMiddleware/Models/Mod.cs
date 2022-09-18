namespace LauncherMiddleware.Models;

public class Mod
{
    public string Uuid { get; set; }
    public int Order { get; set; }
    public bool Active { get; set; }
    public GameName Game { get; set; }
    public string? Packfile { get; set; } // Path to .pack file (data folder)
    public string? Name { get; set; }
    public string? Short { get; set; }
    public string? Category { get; set; }
    public bool Owned { get; set; }
}
