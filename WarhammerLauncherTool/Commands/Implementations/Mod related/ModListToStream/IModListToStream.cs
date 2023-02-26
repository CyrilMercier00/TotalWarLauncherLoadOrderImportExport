﻿using System.Collections.Generic;
using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.ModListToStream;

public interface IModListToStream : ICommand<MemoryStream, List<Mod>> {}
