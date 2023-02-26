using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Steamworks.Data;
using Steamworks.Ugc;

namespace WarhammerLauncherTool.Commands.Implementations.Steam_related.SubscribeToWorkshopItems;

public class SubscribeToWorkshopItem : ISubscribeToWorkshopItems
{
    private ILogger Logger { get; }

    public SubscribeToWorkshopItem(ILogger logger) { Logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    /// <summary>
    /// Subscribe to the given steam worshop items
    /// </summary>
    /// <param name="uuids"></param>
    public async Task ExecuteAsync(ulong[] uuids)
    {
        var entries = await GetEntriesAsync(uuids).ConfigureAwait(false);
        var tasks = entries.Select(workshopItem => workshopItem.Subscribe());

        await Task.WhenAll((IEnumerable<Task>) tasks).ConfigureAwait(false);
    }

    private static async Task<IEnumerable<Item>> GetEntriesAsync(IReadOnlyList<ulong> uuids)
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

        return result?.Entries ?? new List<Item>();
    }
}
