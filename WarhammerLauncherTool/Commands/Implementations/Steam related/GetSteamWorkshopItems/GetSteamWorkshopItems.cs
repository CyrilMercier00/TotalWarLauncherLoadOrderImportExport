using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks.Data;
using Steamworks.Ugc;

namespace WarhammerLauncherTool.Commands.Implementations.Steam_related.GetSteamWorkshopItems;

public class GetSteamWorkshopItems : IGetSteamWorkshopItems
{
    /// <summary>
    /// Retrieve workshop items by uid
    /// </summary>
    /// <param name="uuids"></param>
    public async Task<List<Item>> ExecuteAsync(List<ulong> uuids)
    {
        var fileIds = new PublishedFileId[uuids.Count];
        for (int i = 0; i < uuids.Count; i++)
        {
            ulong id = uuids[i];
            fileIds[i] = new PublishedFileId { Value = id };
        }

        var result = await Query.All
            .WithFileId(fileIds)
            .GetPageAsync(1)
            .ConfigureAwait(false);

        return result?.Entries.ToList() ?? new List<Item>();
    }
}
