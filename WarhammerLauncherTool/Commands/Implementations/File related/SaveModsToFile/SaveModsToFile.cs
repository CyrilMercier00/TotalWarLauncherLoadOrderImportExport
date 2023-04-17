using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;

public class SaveModsToFile : ISaveModsToFile
{
    private readonly ILogger _logger;

    public SaveModsToFile(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    /// <summary>
    /// <see cref="ISaveModsToFile" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns> The path of the file is returned, otherwise an empty string is returned. </returns>
    public string Execute(SaveModsToFileParameters parameters)
    {
        try
        {
            using (var newFileStream = File.Open(parameters.savePath, FileMode.Create))
            {
                string serializedData = JsonConvert.SerializeObject(parameters.mods);
                using var writer = new StreamWriter(newFileStream);
                writer.Write(serializedData);
                writer.Close();
            }

            return parameters.savePath;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unable to save file to {Directory}", parameters.savePath);
            throw;
        }
    }
}
