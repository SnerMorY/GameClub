using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameClub;
public static class BoardGameCatalog
{
    private class Column
    {
        public string Name;
        public int PadRight = 0;
        public int PadLeft = 0;
        public int ColumnWidth = 20;
        public string Separator = "|";

        public string CenterContent(string? content = null)
        {
            if (content == null)
                content = Name ?? string.Empty;
            if (ColumnWidth < (PadRight + PadLeft))
                ColumnWidth = (PadRight + PadLeft);

            if (string.IsNullOrEmpty(content))
                return new string(' ', ColumnWidth);

            if (content.Length >= ColumnWidth)
                return content;

            int leftPadding = (ColumnWidth - content.Length) / 2;
            int rightPadding = ColumnWidth - content.Length - leftPadding;

            return content.PadLeft(leftPadding + content.Length).PadRight(ColumnWidth);
        }
    }
    private readonly static List<BoardGame> boardGames = new List<BoardGame>();
    private readonly static List<Column> boardColumns = new List<Column>() {
        new Column{ Name = "Id", ColumnWidth = 3},
        new Column{ Name = "Название", ColumnWidth = 13},
        new Column{ Name = "Игроки", ColumnWidth = 8},
        new Column{ Name = "Доступно", ColumnWidth = 10},
        new Column{ Name = "Отзывы", ColumnWidth = 15},
    };
    public static void AddBoardGame(BoardGame boardGame)
    {
        boardGames.Add(boardGame);
    }

    public static void ListBoardGames()
    {
        string tableView = GetBoardGamesTable();
        Console.WriteLine(tableView);
    }

    private static string GetTableRow(List<Column> columns, List<string>? contents)
    {
        StringBuilder rowBuilder = new StringBuilder();

        for (int i =  0; i < columns.Count; i++)
        {
            Column column = columns[i];
            string contentText = (contents != null) ? contents[i] : column.Name;

            string content = column.CenterContent(contentText).PadLeft(column.PadLeft).PadRight(column.PadRight);
            rowBuilder.Append(content);
            rowBuilder.Append($" {column.Separator} ");
        }

        return rowBuilder.ToString();
    }

    private static string GetBoardGameRow(List<Column> columns, BoardGame boardGame)
    {
        List<string> contents = new List<string>()
        {
            boardGame.Id.ToString(),
            boardGame.Name,
            boardGame.CurrentPlayers.ToString(),
            boardGame.Available.ToString(),
            boardGame.Reviews.ToString()
        };

        return GetTableRow(columns, contents);
    }
    private static string GetTableHeader()
    {
        return GetTableRow(boardColumns, null);
    }
    public static string GetBoardGamesTable(List<BoardGame> boardGamesToDisplay)
    {
        //ID | Название | Игроки | Доступно | Отзывы
        StringBuilder tableBuilder = new StringBuilder();
        tableBuilder.AppendLine(GetTableHeader());

        foreach (var boardGame in boardGamesToDisplay)
        {
            string boardGameRow = GetBoardGameRow(boardColumns, boardGame);
            tableBuilder.AppendLine(boardGameRow);
        }

        return tableBuilder.ToString();
    }

    public static string GetBoardGamesTable()
    {
        return GetBoardGamesTable(boardGames);
    }

    public static string GetFilteredBoardGamesTable(Func<BoardGame, bool> predicate)
    {
        List<BoardGame> filteredBoardGames = new List<BoardGame>();
        foreach (var boardGame in boardGames)
        {
            if (predicate(boardGame))
                filteredBoardGames.Add(boardGame);
        }
        return GetBoardGamesTable(filteredBoardGames);
    }
}

