using System;

namespace WarhammerLauncherTool.Commands;

public class CommandInvoker
{
    private readonly ICommand _command;

    public void SetCommand(ICommand command) => _command = command;

    public void ExecuteCommand()
    {
        if (_command != null)
        {
            return _command.Execute();
        }

        throw new NullReferenceException();
    }
}
