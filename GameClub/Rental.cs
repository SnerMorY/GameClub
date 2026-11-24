using System;

namespace GameClub;

public class Rental
{
    public int GameId { get; private set; }
    public string GameName { get; private set; }
    public string CustomerName { get; private set; }
    public DateTime RentedAt { get; private set; }

    public Rental(int gameId, string gameName, string customerName)
    {
        GameId = gameId;
        GameName = gameName;
        CustomerName = customerName.Trim();
        RentedAt = DateTime.Now;
    }
}