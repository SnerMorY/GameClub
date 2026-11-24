namespace GameClub;

public class BoardGame
{
    private static int Count = 0;
    public int Id { get; private set; }
    public string Name { get; set; }
    public int MinPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public int CurrentPlayers { get; private set; }
    public int Available { get; private set; }
    public readonly List<Review> Reviews;
    public BoardGame(string name, int minPlayers, int maxPlayers)
    {
        Id = Count++;
        Name = name;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        Reviews = new List<Review>();
    }

    public BoardGame(string name) : this(name, 2, 4) { }

    public bool TakeGame(int? amountPlayers)
    {
        if (Available <= 0)
            return false;

        if (amountPlayers.HasValue)
            CurrentPlayers += amountPlayers.Value;
        else
            CurrentPlayers += 1;
        
        Available -= 1;
        return true;
    }
}