namespace LauncherMiddleware.Models;

public abstract class Mod
{
    public string Uuid { get; set; }
    public int Order { get; set; }
    public bool Active { get; set; }
    public GameName Game { get; set; }
    public string? Packfile { get; set; } // Path to .pack file (point to the game folder)
    public string? Name { get; set; }
    public string? Short { get; set; }
    public string? Category { get; set; }
    public bool Owned { get; set; }
}
