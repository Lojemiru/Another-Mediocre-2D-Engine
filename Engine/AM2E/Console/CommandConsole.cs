using System;
using System.Collections.Generic;
using System.Threading;

namespace AM2E;

public static class CommandConsole
{
    private static Dictionary<string, Action<string[]>> commands = new();
    private static Dictionary<string, string> descriptions = new();
    private static Dictionary<string, string> syntaxes = new();
    private static List<DeferredCommand> deferredCommands = new();
    private static bool stopThread = false;
    
    /// <summary>
    /// Static constructor that adds the built-in AM2E commands.
    /// </summary>
    static CommandConsole()
    {
        Add("help", "Displays this message. Call with a command to get its syntax.", "[command]", (args =>
        {
            // No argument - list all commands
            if (args.Length < 1)
            {
                Console.WriteLine("[AM2E " + EngineCore.Version + " Command Console]\n");
                foreach (var command in commands.Keys)
                    Console.WriteLine(command);
            }
            else
            {
                Console.WriteLine(args[0] + ": " + args[0] + " " + syntaxes[args[0]] + "\n    " + descriptions[args[0]]);
            }
        }));
        
        Add("clear", "Clears the console.", "", (args => Console.Clear()));
    }
    
    /// <summary>
    /// Starts running the <see cref="CommandConsole"/>.
    /// </summary>
    public static void Start()
    {
        var t = new Thread(MainLoop)
        {
            IsBackground = true
        };
        
        t.Start();
    }

    public static void Stop()
    {
        stopThread = true;
    }

    private static void MainLoop()
    {
        while (!stopThread)
        {
            Console.Write("> ");
            ParseCommand(Console.ReadLine());
        }

        stopThread = false;
        Console.Write("Exiting AM2E Command Console!");
    }
    
    
    public static void Add(string name, string description, string syntax, Action<string[]> handler)
    {
        commands.Add(name, handler);
        descriptions.Add(name, description);
        syntaxes.Add(name, syntax);
    }

    private static void ParseCommand(string command)
    {
        if (command.Length < 1)
            return;

        var split = command.Split(" ");
        
        // Show error and return if command is not defined
        if (!commands.ContainsKey(split[0]))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: Command \"" + split[0] + "\" does not exist!");
            Console.ResetColor();
            return;
        }

        var input = new string[split.Length - 1];

        for (var i = 0; i < input.Length; i++)
        {
            input[i] = split[i + 1];
        }
        
        commands[split[0]](input);
    }

    internal static void ExecuteDeferredCommands()
    {
        foreach (var command in deferredCommands)
        {
            command.Execute();
        }
        
        deferredCommands.Clear();
    }
}