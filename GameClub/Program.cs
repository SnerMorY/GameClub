using System;
using System.Collections.Generic;
using System.Text;

namespace GameClub;

public class Program
{
    static bool isRunning = true;

    static void Main()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            InitCatalog();
            RegisterCommands();

            while (isRunning)
            {
                PrintMenu();
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                List<string> tokens = ParseInput(input);
                if (tokens.Count == 0)
                    continue;

                string cmdName = tokens[0];
                List<string> args = tokens.GetRange(1, tokens.Count - 1);

                Console.WriteLine();
                if (Command.TryExtractCommand(cmdName, out Command cmd))
                {
                    cmd.Execute(args);
                }
                else
                {
                    Console.WriteLine($"Ошибка: неизвестная команда '{cmdName}'. Введите 'help' или номер пункта меню.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
        }
    }

    static void InitCatalog()
    {
        string[] lines =
        {
            "Каркассон;2;5;2",
            "Манчкин;3;6;1",
            "Диксит;3;6;2",
            "Эволюция;2;4;1",
            "Банг;4;7;1"
        };

        BoardGameCatalog.LoadFromLines(lines);
    }

    static void PrintMenu()
    {
        Console.WriteLine("\n=== ИГРОВОЙ КЛУБ ===");
        Console.WriteLine("1. Показать каталог (list)");
        Console.WriteLine("2. Добавить игру (add)");
        Console.WriteLine("3. Поиск по названию (search)");
        Console.WriteLine("4. Фильтры каталога (filter)");
        Console.WriteLine("5. Выдать игру (take)");
        Console.WriteLine("6. Вернуть игру (return)");
        Console.WriteLine("7. Добавить отзыв (review)");
        Console.WriteLine("8. Топ-3 игры по рейтингу (top)");
        Console.WriteLine("9. Удалить игру навсегда (delete)");
        Console.WriteLine("h. Справка по командам и алиасам (help)");
        Console.WriteLine("0. Выход (exit)");
        Console.Write("Выберите действие: ");
    }

    static void RegisterCommands()
    {
        new Command("list", "Вывести полный каталог настольных игр", "list", 0,
            (args) => BoardGameCatalog.ListBoardGames(), "1", "ls", "catalog");

        new Command("add", "Добавить новую игру в каталог", "add [\"Название\" мин макс копий]", 4,
            (args) =>
            {
                string name;
                int minP, maxP, copies;

                if (args.Count == 4)
                {
                    name = args[0];
                    if (!int.TryParse(args[1], out minP) || !int.TryParse(args[2], out maxP) || !int.TryParse(args[3], out copies))
                    {
                        Console.WriteLine("Ошибка: числовые параметры введены некорректно.");
                        return;
                    }
                }
                else
                {
                    Console.Write("Введите название игры: ");
                    name = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("Ошибка: название не может быть пустым.");
                        return;
                    }

                    if (!TryReadInt("Минимальное число игроков: ", out minP) ||
                        !TryReadInt("Максимальное число игроков: ", out maxP) ||
                        !TryReadInt("Количество копий: ", out copies))
                        return;
                }

                if (minP <= 0 || maxP < minP || copies <= 0)
                {
                    Console.WriteLine("Ошибка: проверьте корректность диапазона игроков и количества копий.");
                    return;
                }

                BoardGame game = new BoardGame(name, minP, maxP, copies);
                BoardGameCatalog.AddBoardGame(game);
                Console.WriteLine($"Игра «{game.Name}» успешно добавлена под ID {game.Id}.");
            }, "2", "new");

        new Command("search", "Поиск игр по части названия (без учета регистра)", "search [\"подстрока\"]", 1,
            (args) =>
            {
                string query = args.Count == 1 ? args[0] : ReadString("Введите название (или его часть): ");
                if (string.IsNullOrWhiteSpace(query))
                {
                    Console.WriteLine("Ошибка: строка поиска не может быть пустой.");
                    return;
                }

                string table = BoardGameCatalog.GetFilteredBoardGamesTable(
                    g => g.Name.Contains(query, StringComparison.OrdinalIgnoreCase),
                    $"ПОИСК ПО НАЗВАНИЮ: «{query}»");
                Console.WriteLine(table);
            }, "3", "find");

        new Command("filter", "Фильтрация игр (1 - по числу игроков, 2 - только доступные, 3 - по алфавиту)", "filter [players <N> | free | alpha]", 2,
            (args) =>
            {
                string mode;
                if (args.Count >= 1)
                {
                    mode = args[0].ToLower();
                }
                else
                {
                    Console.WriteLine("Выберите критерий фильтрации:");
                    Console.WriteLine("  1. По числу игроков (подходит для N участников)");
                    Console.WriteLine("  2. Только доступные игры (Available > 0)");
                    Console.WriteLine("  3. Все игры с сортировкой по алфавиту");
                    Console.Write("Критерий: ");
                    mode = Console.ReadLine()?.Trim().ToLower() ?? "";
                }

                if (mode == "1" || mode == "players")
                {
                    int playersCount;
                    if (args.Count == 2)
                    {
                        if (!int.TryParse(args[1], out playersCount))
                        {
                            Console.WriteLine("Ошибка: количество игроков должно быть числом.");
                            return;
                        }
                    }
                    else if (!TryReadInt("Введите количество участников: ", out playersCount))
                    {
                        return;
                    }

                    Console.WriteLine(BoardGameCatalog.GetFilteredBoardGamesTable(
                        g => playersCount >= g.MinPlayers && playersCount <= g.MaxPlayers,
                        $"ИГРЫ ДЛЯ {playersCount} УЧАСТНИКОВ"));
                }
                else if (mode == "2" || mode == "free")
                {
                    Console.WriteLine(BoardGameCatalog.GetFilteredBoardGamesTable(
                        g => g.Available > 0,
                        "ДОСТУПНЫЕ ДЛЯ ВЫДАЧИ ИГРЫ"));
                }
                else if (mode == "3" || mode == "alpha")
                {
                    Console.WriteLine(BoardGameCatalog.GetFilteredBoardGamesTable(
                        g => true,
                        "КАТАЛОГ ПО АЛФАВИТУ",
                        sortByName: true));
                }
                else
                {
                    Console.WriteLine("Ошибка: неизвестный критерий фильтрации.");
                }
            }, "4");

        new Command("take", "Выдать игру покупателю", "take [id \"ФИО\"]", 2,
            (args) =>
            {
                int id;
                string customer;

                if (args.Count == 2)
                {
                    if (!int.TryParse(args[0], out id))
                    {
                        Console.WriteLine("Ошибка: ID должен быть числом.");
                        return;
                    }
                    customer = args[1];
                }
                else
                {
                    if (!TryReadInt("Введите ID игры: ", out id))
                        return;
                    customer = ReadString("Введите ФИО покупателя: ");
                }

                BoardGameCatalog.RentGame(id, customer, out string message);
                Console.WriteLine(message);
            }, "5", "rent", "get");

        new Command("return", "Принять игру обратно с проверкой целостности", "return [id \"ФИО\" да/нет]", 3,
            (args) =>
            {
                int id;
                string customer;
                string intactInput;

                if (args.Count == 3)
                {
                    if (!int.TryParse(args[0], out id))
                    {
                        Console.WriteLine("Ошибка: ID должен быть числом.");
                        return;
                    }
                    customer = args[1];
                    intactInput = args[2].Trim().ToLower();
                }
                else
                {
                    if (!TryReadInt("Введите ID игры: ", out id))
                        return;
                    customer = ReadString("Введите ФИО покупателя: ");
                    intactInput = ReadString("Игра цела? (да/нет): ").Trim().ToLower();
                }

                bool isIntact;
                if (intactInput == "да" || intactInput == "yes" || intactInput == "y" || intactInput == "+")
                    isIntact = true;
                else if (intactInput == "нет" || intactInput == "no" || intactInput == "n" || intactInput == "-")
                    isIntact = false;
                else
                {
                    Console.WriteLine("Ошибка: введите 'да' или 'нет' для подтверждения целостности.");
                    return;
                }

                BoardGameCatalog.ReturnGame(id, customer, isIntact, out string message);
                Console.WriteLine(message);
            }, "6", "ret", "back");

        new Command("review", "Добавить отзыв и оценку (1–5) к игре", "review [id \"ФИО\" оценка \"текст\"]", 4,
            (args) =>
            {
                int id, rating;
                string author, text;

                if (args.Count == 4)
                {
                    if (!int.TryParse(args[0], out id) || !int.TryParse(args[2], out rating))
                    {
                        Console.WriteLine("Ошибка: ID и оценка должны быть числами.");
                        return;
                    }
                    author = args[1];
                    text = args[3];
                }
                else
                {
                    if (!TryReadInt("Введите ID игры: ", out id))
                        return;
                    author = ReadString("Введите ФИО покупателя: ");
                    if (!TryReadInt("Оценка (от 1 до 5): ", out rating))
                        return;
                    text = ReadString("Текст отзыва: ");
                }

                if (rating < 1 || rating > 5)
                {
                    Console.WriteLine("Ошибка: оценка должна быть в диапазоне от 1 до 5.");
                    return;
                }

                BoardGame? game = BoardGameCatalog.GetBoardGameById(id);
                if (game == null)
                {
                    Console.WriteLine($"Ошибка: игра с ID {id} не найдена.");
                    return;
                }

                game.AddReview(new Review(author, rating, text));
                Console.WriteLine($"Отзыв для «{game.Name}» добавлен (оценка {rating}).");
            }, "7", "rev");

        new Command("top", "Показать топ лучших игр по средней оценке", "top [количество]", 1,
            (args) =>
            {
                int limit = 3;
                if (args.Count == 1 && (!int.TryParse(args[0], out limit) || limit <= 0))
                {
                    Console.WriteLine("Ошибка: количество игр в топе должно быть положительным числом.");
                    return;
                }
                Console.WriteLine(BoardGameCatalog.GetTopGamesTable(limit));
            }, "8", "rating", "leaders");

        new Command("delete", "Удалить игру из каталога навсегда", "delete [id]", 1,
            (args) =>
            {
                int id;
                if (args.Count == 1)
                {
                    if (!int.TryParse(args[0], out id))
                    {
                        Console.WriteLine("Ошибка: ID должен быть числом.");
                        return;
                    }
                }
                else if (!TryReadInt("Введите ID игры для удаления: ", out id))
                {
                    return;
                }

                BoardGame? game = BoardGameCatalog.GetBoardGameById(id);
                if (game == null)
                {
                    Console.WriteLine($"Ошибка: игра с ID {id} не найдена.");
                    return;
                }

                if (BoardGameCatalog.HasActiveRentals(id))
                {
                    string confirm = ReadString($"Внимание: копии игры «{game.Name}» сейчас на руках! Точно удалить? (да/нет): ").ToLower();
                    if (confirm != "да" && confirm != "yes")
                    {
                        Console.WriteLine("Удаление отменено.");
                        return;
                    }
                }

                string removedName = game.Name;
                BoardGameCatalog.RemoveBoardGame(id);
                Console.WriteLine($"Игра «{removedName}» (ID {id}) навсегда удалена из каталога.");
            }, "9", "del", "rm");

        new Command("help", "Показать справку по всем командам и алиасам", "help", 0,
            (args) => Command.PrintHelp(), "h", "?");

        new Command("exit", "Завершение работы программы", "exit", 0,
            (args) =>
            {
                Console.WriteLine("Работа программы завершена.");
                isRunning = false;
            }, "0", "quit", "q");
    }

    static bool TryReadInt(string prompt, out int result)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out result))
        {
            Console.WriteLine("Ошибка: необходимо ввести целое число.");
            return false;
        }
        return true;
    }

    static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    static List<string> ParseInput(string input)
    {
        List<string> result = new List<string>();
        StringBuilder currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentToken.Length > 0)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                currentToken.Append(c);
            }
        }

        if (currentToken.Length > 0)
            result.Add(currentToken.ToString());

        return result;
    }
}