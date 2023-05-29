using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;

public class SaveModsToFile : ISaveModsToFile
{
    private readonly ILogger _logger;

    public SaveModsToFile(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    /// <summary>
    /// <see cref="ISaveModsToFile" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns> The path to the file is returned </returns>
    public string Execute(SaveModsToFileParameters parameters)
    {
        try
        {
            using (var newFileStream = File.Open(parameters.SavePath, FileMode.Create))
            {
                string serializedData = JsonConvert.SerializeObject(parameters.Mods);
                using var writer = new StreamWriter(newFileStream);
                writer.Write(serializedData);
                writer.Close();
            }

            return parameters.SavePath;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unable to save file to {Directory}", parameters.SavePath);
            throw;
        }
    }
}
