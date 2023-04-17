using System.Threading.Tasks;

namespace WarhammerLauncherTool.Commands;

public interface ICommandAsync <TOut, in TIn>
{
    Task<TOut> ExecuteAsync(TIn input);
}

public interface ICommandAsync <in TIn>
{
    Task ExecuteAsync(TIn input);
}

public interface ICommandAsync
{
    Task ExecuteAsync();
}
