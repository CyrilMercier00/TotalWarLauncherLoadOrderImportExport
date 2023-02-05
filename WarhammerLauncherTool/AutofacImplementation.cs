using Autofac;
using Serilog;

namespace WarhammerLauncherTool;

public abstract class AutofacImplementation
{
    public static void Initialize()
    {
        // Create your builder.
        var builder = new ContainerBuilder();

        // Serilog
        builder.Register<ILogger>((_, _) =>
                new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("logs/log-{Date}.txt", retainedFileCountLimit: 5)
                    .CreateLogger())
            .SingleInstance();
    }
}
