using System.Threading.Tasks;

namespace WarhammerLauncherTool.Commands;

public interface ICommandAsync <in TIn>
{
    Task ExecuteAsync(TIn input);
}
