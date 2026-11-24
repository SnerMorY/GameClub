using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameClub;

public class Command
{
    private static Dictionary<string, Command> Commands = new Dictionary<string, Command>();
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool ArgsRequired { get; set; } = true;
    public int MinArgs { get; private set; }
    public int MaxArgs { get; private set; }
    private Action<List<string>> CmdFunc;
    
    static string FormatCommandName(string name)
    {
        return name.Trim().ToLower();
    }

    public static Command? ExtractCommand(List<string> args)
    {
        if (args == null)
            return null;
        if (args.Count == 0)
            return null;

        string cmdName = FormatCommandName(args[0]);
        if (Commands.ContainsKey(cmdName))
            return Commands[cmdName];
        return null;
    }

    public static bool TryExtractCommand(List<string> args, out Command cmd)
    {
        Command extractedCmd = ExtractCommand(args);
        if (extractedCmd == null)
        {
            cmd = null;
            return false;
        } else
        {
            cmd = extractedCmd;
            return true;
        }
    }
    public Command(string name, string desc, int minArgs, int maxArgs, bool argsRequired, Action<List<string>> cmdFunc)
    {
        Name = FormatCommandName(name);
        Description = desc;
        MinArgs = minArgs;
        MaxArgs = maxArgs;
        ArgsRequired = argsRequired;
        CmdFunc = cmdFunc;

        Commands.Add(name, this);
    }

    public Command(string name, bool argsRequired=true) : this(name, "", -1, -1, argsRequired, (args) => Console.WriteLine(name)) { }
    
    ~Command()
    {
        Commands.Remove(Name);
    }
    public void Execute(List<string>? args)
    {
        if (args == null && ArgsRequired)
            return;
        if (MinArgs != -1 && args.Count < MinArgs)
            return;
        if (MaxArgs != -1 && args.Count > MaxArgs)
            return;

        CmdFunc(args);
    }
    public void Execute()
    {
        if (!ArgsRequired)
        {
            this.Execute(null);
        }
    }
}
