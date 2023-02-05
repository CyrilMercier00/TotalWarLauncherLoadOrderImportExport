namespace WarhammerLauncherTool.Commands;

public interface ICommand <in TIn, out TOut>
{
    TOut Execute(TIn input);
}
