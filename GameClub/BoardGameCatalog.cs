using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GameClub;

public enum TextAlign
{
    Left,
    Center,
    Right
}

public static class BoardGameCatalog
{
    private class Column
    {
        public string Name = string.Empty;
        public int PadRight = 0;
        public int PadLeft = 0;
        public int ColumnWidth = 20;
        public string Separator = "|";
        public TextAlign Align = TextAlign.Center;

        public string FormatContent(string? content = null)
        {
            if (content == null)
                content = Name ?? string.Empty;

            if (ColumnWidth < (PadRight + PadLeft))
                ColumnWidth = (PadRight + PadLeft);

            if (string.IsNullOrEmpty(content))
                return new string(' ', ColumnWidth);

            if (content.Length > ColumnWidth)
            {
                if (ColumnWidth > 3)
                    content = content.Substring(0, ColumnWidth - 3) + "...";
                else
                    content = content.Substring(0, ColumnWidth);
            }

            if (Align == TextAlign.Left)
                return content.PadRight(ColumnWidth);

            if (Align == TextAlign.Right)
                return content.PadLeft(ColumnWidth);

            int leftPadding = (ColumnWidth - content.Length) / 2;
            return content.PadLeft(leftPadding + content.Length).PadRight(ColumnWidth);
        }
    }

    private readonly static List<BoardGame> boardGames = new List<BoardGame>();
    private readonly static List<Rental> activeRentals = new List<Rental>();
    public static int TotalSuccessfulRentals { get; private set; } = 0;

    private readonly static List<Column> boardColumns = new List<Column>()
    {
        new Column { Name = "ID", ColumnWidth = 4, Align = TextAlign.Left },
        new Column { Name = "Название", ColumnWidth = 18, Align = TextAlign.Left },
        new Column { Name = "Игроки", ColumnWidth = 8, Align = TextAlign.Left },
        new Column { Name = "Доступно", ColumnWidth = 9, Align = TextAlign.Left },
        new Column { Name = "Отзывы", ColumnWidth = 16, Align = TextAlign.Left }
    };

    private readonly static List<Column> topColumns = new List<Column>()
    {
        new Column { Name = "Место", ColumnWidth = 6, Align = TextAlign.Center },
        new Column { Name = "ID", ColumnWidth = 4, Align = TextAlign.Center },
        new Column { Name = "Название", ColumnWidth = 18, Align = TextAlign.Left },
        new Column { Name = "Рейтинг", ColumnWidth = 8, Align = TextAlign.Center },
        new Column { Name = "Отзывов", ColumnWidth = 8, Align = TextAlign.Center }
    };

    public static void LoadFromLines(string[] lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length < 4)
                continue;

            string name = parts[0].Trim();
            if (int.TryParse(parts[1], out int minP) &&
                int.TryParse(parts[2], out int maxP) &&
                int.TryParse(parts[3], out int copies))
            {
                AddBoardGame(new BoardGame(name, minP, maxP, copies));
            }
        }
    }

    public static void AddBoardGame(BoardGame boardGame)
    {
        boardGames.Add(boardGame);
    }

    public static bool RemoveBoardGame(int id)
    {
        BoardGame? game = GetBoardGameById(id);
        if (game == null)
            return false;

        boardGames.Remove(game);
        activeRentals.RemoveAll(r => r.GameId == id);
        return true;
    }

    public static BoardGame? GetBoardGameById(int id)
    {
        foreach (var game in boardGames)
        {
            if (game.Id == id)
                return game;
        }
        return null;
    }

    public static bool HasActiveRentals(int gameId)
    {
        return activeRentals.Any(r => r.GameId == gameId);
    }

    public static bool RentGame(int gameId, string customerName, out string message)
    {
        BoardGame? game = GetBoardGameById(gameId);
        if (game == null)
        {
            message = $"Ошибка: игра с ID {gameId} не найдена.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            message = "Ошибка: ФИО покупателя не может быть пустым.";
            return false;
        }

        if (!game.TakeCopy())
        {
            message = $"Ошибка: нет свободных копий игры «{game.Name}».";
            return false;
        }

        activeRentals.Add(new Rental(game.Id, game.Name, customerName));
        TotalSuccessfulRentals++;
        message = $"Выдача «{game.Name}» покупателю {customerName.Trim()}: успешно.";
        return true;
    }

    public static bool ReturnGame(int gameId, string customerName, bool isIntact, out string message)
    {
        BoardGame? game = GetBoardGameById(gameId);
        if (game == null)
        {
            message = $"Ошибка: игра с ID {gameId} не найдена.";
            return false;
        }

        Rental? rental = activeRentals.FirstOrDefault(r =>
            r.GameId == gameId &&
            r.CustomerName.Equals(customerName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (rental == null)
        {
            message = $"Ошибка: покупатель {customerName.Trim()} не брал игру «{game.Name}».";
            return false;
        }

        if (!game.ReturnCopy(isIntact))
        {
            message = $"Ошибка: невозможно вернуть игру «{game.Name}» (все копии уже учтены).";
            return false;
        }

        activeRentals.Remove(rental);

        if (isIntact)
            message = $"Возврат «{game.Name}» от {rental.CustomerName}: игра цела, копия возвращена.";
        else
            message = $"Возврат «{game.Name}» от {rental.CustomerName}: ВНИМАНИЕ! Игра повреждена и списана (доступное количество не увеличено).";

        return true;
    }

    public static void ListBoardGames()
    {
        Console.WriteLine(GetBoardGamesTable(boardGames, "ИГРОВОЙ КЛУБ — КАТАЛОГ"));
    }

    private static string GetTableRow(List<Column> columns, List<string>? contents)
    {
        StringBuilder rowBuilder = new StringBuilder();

        for (int i = 0; i < columns.Count; i++)
        {
            Column column = columns[i];
            string contentText = (contents != null) ? contents[i] : column.Name;

            string content = column.FormatContent(contentText);
            if (column.PadLeft > 0)
                content = new string(' ', column.PadLeft) + content;
            if (column.PadRight > 0)
                content = content + new string(' ', column.PadRight);

            rowBuilder.Append(content);
            if (i < columns.Count - 1)
                rowBuilder.Append($" {column.Separator} ");
        }

        return rowBuilder.ToString();
    }

    private static string GetBoardGameRow(List<Column> columns, BoardGame boardGame)
    {
        string playersInfo = $"{boardGame.MinPlayers}–{boardGame.MaxPlayers}";
        string availableInfo = $"{boardGame.Available}/{boardGame.TotalCopies}";
        string reviewsInfo = boardGame.GetReviewsSummary();

        List<string> contents = new List<string>()
        {
            boardGame.Id.ToString(),
            boardGame.Name,
            playersInfo,
            availableInfo,
            reviewsInfo
        };

        return GetTableRow(columns, contents);
    }

    public static string GetBoardGamesTable(IEnumerable<BoardGame> gamesToDisplay, string title = "ИГРОВОЙ КЛУБ — КАТАЛОГ")
    {
        var list = gamesToDisplay.ToList();
        string header = GetTableRow(boardColumns, null);
        int lineWidth = Math.Max(header.Length, 58);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine(new string('=', lineWidth));
        sb.AppendLine(header);
        sb.AppendLine(new string('-', lineWidth));

        if (list.Count == 0)
        {
            sb.AppendLine("Игры по заданным условиям не найдены.");
        }
        else
        {
            foreach (var game in list)
            {
                sb.AppendLine(GetBoardGameRow(boardColumns, game));
            }
        }

        sb.AppendLine(new string('=', lineWidth));
        sb.AppendLine($"Всего успешных выдач: {TotalSuccessfulRentals}");
        return sb.ToString();
    }

    public static string GetFilteredBoardGamesTable(Func<BoardGame, bool> predicate, string title = "РЕЗУЛЬТАТЫ ПОИСКА / ФИЛЬТРАЦИИ", bool sortByName = false)
    {
        IEnumerable<BoardGame> query = boardGames.Where(predicate);
        if (sortByName)
            query = query.OrderBy(g => g.Name);

        return GetBoardGamesTable(query, title);
    }

    public static string GetTopGamesTable(int limit = 3)
    {
        var ratedGames = boardGames
            .Where(g => g.Reviews.Count > 0)
            .OrderByDescending(g => g.GetAverageRating())
            .ThenByDescending(g => g.Reviews.Count)
            .Take(limit)
            .ToList();

        if (ratedGames.Count == 0)
            return "Пока ни у одной игры нет оценок для формирования топа.";

        string header = GetTableRow(topColumns, null);
        int lineWidth = Math.Max(header.Length, 54);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"ТОП-{limit} ИГР ПО РЕЙТИНГУ");
        sb.AppendLine(new string('=', lineWidth));
        sb.AppendLine(header);
        sb.AppendLine(new string('-', lineWidth));

        int place = 1;
        foreach (var game in ratedGames)
        {
            string avg = game.GetAverageRating().ToString("0.0", CultureInfo.InvariantCulture);
            List<string> contents = new List<string>()
            {
                place.ToString(),
                game.Id.ToString(),
                game.Name,
                $"{avg}/5",
                game.Reviews.Count.ToString()
            };
            sb.AppendLine(GetTableRow(topColumns, contents));
            place++;
        }

        sb.AppendLine(new string('=', lineWidth));
        return sb.ToString();
    }
}