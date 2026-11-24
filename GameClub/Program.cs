using System.Data;

namespace GameClub;

public class Program
{
    static void Main()
    {
        BoardGame bg = new BoardGame("igra");
        BoardGameCatalog.AddBoardGame(bg);

        Command testCmd = new Command("list", "list", -1, -1, false, (args)=>BoardGameCatalog.ListBoardGames());
        //testCmd.Execute();

        BoardGameCatalog.ListBoardGames();

        bool isRunning = true;
        while (isRunning)
        {
            Console.Write("Введите: ");
            string prompt = Console.ReadLine().Trim();
            var args = new List<string>() { prompt };

            bool success = Command.TryExtractCommand(args, out Command cmd);
            if (success)
                cmd.Execute();
        }
    }
}