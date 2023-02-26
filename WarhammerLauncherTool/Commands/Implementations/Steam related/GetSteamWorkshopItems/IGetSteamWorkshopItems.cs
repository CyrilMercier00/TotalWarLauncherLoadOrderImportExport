using System.Collections.Generic;
using Steamworks.Ugc;

namespace WarhammerLauncherTool.Commands.Implementations.Steam_related.SubscribeToWorkshopItems;

public interface IGetSteamWorkshopItems : ICommandAsync<List<Item>, List<ulong>> {}
