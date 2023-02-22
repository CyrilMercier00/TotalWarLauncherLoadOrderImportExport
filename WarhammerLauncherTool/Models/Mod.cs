// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

#pragma warning disable CS8618
namespace WarhammerLauncherTool.Models;

public class Mod
{
    public bool Active { get; set; }
    public string Category { get; set; }
    public GameName Game { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public bool Owned { get; set; }
    public string PackFile { get; set; }
    public string Uuid { get; set; }
    public string Short { get; set; }

    private ulong? _workshopUid;

    public ulong workshopUid
    {
        get
        {
            if (_workshopUid is null)
            {
                string uid = PackFile.Split('/')[^2]; // last is the mod name, before last is the actual id
                _workshopUid = (ulong) long.Parse(uid);
            }

            return (ulong) _workshopUid;
        }
    }
}
