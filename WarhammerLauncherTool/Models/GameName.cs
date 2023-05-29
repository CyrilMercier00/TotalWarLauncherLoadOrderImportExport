using System.Runtime.Serialization;

namespace WarhammerLauncherTool.Models;

public enum GameName
{
    Attila,
    Britannia,
    Empire,
    Medieval,
    Medieval2,
    Napoleon,
    Rome,
    Rome2,
    Shogun,
    Shogun2,
    Threekingdoms,
    [EnumMember(Value = "11")] Warhammer3,
    Warhammer2,
    Warhammer
}
