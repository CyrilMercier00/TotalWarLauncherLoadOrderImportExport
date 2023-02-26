namespace WarhammerLauncherTool.Commands;

public interface ICommand <out TOut, in TIn>
{
    TOut Execute(TIn input);
}

public interface ICommand <out TOut>
{
    TOut Execute();
}
