using System;
using System.Collections.Generic;

namespace GameClub;

public class Command
{
    private static readonly Dictionary<string, Command> Commands = new Dictionary<string, Command>();
    private static readonly List<Command> UniqueCommands = new List<Command>();

    public string Name { get; private set; }
    public List<string> Aliases { get; private set; }
    public string Description { get; private set; }
    public string Usage { get; private set; }
    public int MaxArgs { get; private set; }
    private readonly Action<List<string>> CmdFunc;

    static string FormatCommandName(string name)
    {
        return name.Trim().ToLower();
    }

    public static Command? ExtractCommand(string rawCommandName)
    {
        if (string.IsNullOrWhiteSpace(rawCommandName))
            return null;

        string cmdName = FormatCommandName(rawCommandName);
        if (Commands.TryGetValue(cmdName, out Command? cmd))
            return cmd;

        return null;
    }

    public static bool TryExtractCommand(string rawCommandName, out Command cmd)
    {
        Command? extractedCmd = ExtractCommand(rawCommandName);
        if (extractedCmd == null)
        {
            cmd = null!;
            return false;
        }
        cmd = extractedCmd;
        return true;
    }

    public Command(string name, string desc, string usage, int maxArgs, Action<List<string>> cmdFunc, params string[] aliases)
    {
        Name = FormatCommandName(name);
        Description = desc;
        Usage = usage;
        MaxArgs = maxArgs;
        CmdFunc = cmdFunc;
        Aliases = new List<string>();

        Commands[Name] = this;
        UniqueCommands.Add(this);

        foreach (var alias in aliases)
        {
            AddAlias(alias);
        }
    }

    public void AddAlias(string alias)
    {
        string formatted = FormatCommandName(alias);
        if (!string.IsNullOrEmpty(formatted) && !Commands.ContainsKey(formatted))
        {
            Aliases.Add(formatted);
            Commands[formatted] = this;
        }
    }

    public void Execute(List<string>? args)
    {
        List<string> safeArgs = args ?? new List<string>();
        if (MaxArgs != -1 && safeArgs.Count > MaxArgs)
        {
            Console.WriteLine($"Ошибка: слишком много аргументов (максимум {MaxArgs}). Использование: {Usage}");
            return;
        }

        CmdFunc(safeArgs);
    }

    public static void PrintHelp()
    {
        Console.WriteLine("\n=== СПРАВКА ПО КОМАНДАМ ===");
        Console.WriteLine("Можно вводить как номер пункта меню, так и текстовую команду (с аргументами или без):\n");
        foreach (var cmd in UniqueCommands)
        {
            string aliasStr = cmd.Aliases.Count > 0 ? $" [алиасы: {string.Join(", ", cmd.Aliases)}]" : "";
            Console.WriteLine($"  {cmd.Usage}{aliasStr}");
            Console.WriteLine($"      {cmd.Description}");
        }
    }
}