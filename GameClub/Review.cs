using System;

namespace GameClub;

public class Review
{
    public string Author { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }

    public Review(string author, int rating, string comment)
    {
        Author = string.IsNullOrWhiteSpace(author) ? "Аноним" : author.Trim();
        Rating = Math.Clamp(rating, 1, 5);
        Comment = comment?.Trim() ?? string.Empty;
    }

    public override string ToString()
    {
        return $"[{Rating}/5] {Author}: {Comment}";
    }
}