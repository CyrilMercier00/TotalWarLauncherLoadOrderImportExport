// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS8618
namespace WarhammerLauncherTool.Models;

public class Mod
{
    public bool Active { get; set; }
    public string Category { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public GameName Game { get; set; }

    public string Name { get; set; }
    public int Order { get; set; }
    public bool Owned { get; set; }
    public string PackFile { get; set; }
    public string Uuid { get; set; }
    public string Short { get; set; }
}
