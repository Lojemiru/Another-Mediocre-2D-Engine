using System;

namespace AM2E;

internal class DeferredCommand
{
    private Action<string[]> action;
    private string[] args;
    
    internal DeferredCommand(Action<string[]> command, string[] args)
    {
        action = command;
        this.args = args;
    }

    internal void Execute() => action(args);
}