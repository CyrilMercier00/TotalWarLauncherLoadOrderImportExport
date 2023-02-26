using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks.Ugc;

namespace WarhammerLauncherTool.Commands.Implementations.Steam_related.SubscribeToWorkshopItems;

public class SubscribeToWorkshopItem : ISubscribeToWorkshopItems
{
    /// <summary>
    /// Subscribe to the given steam worshop items
    /// </summary>
    /// <param name="uuids"></param>
    public Task ExecuteAsync(List<Item> items)
    {
        var tasks = new List<Task>();

        foreach (var item in items)
        {
            tasks.Add(item.Subscribe());
        }

        return Task.WhenAll(tasks);
    }
}
