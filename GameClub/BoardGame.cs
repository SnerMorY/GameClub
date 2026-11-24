using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameClub;

public class BoardGame
{
    private static int NextId = 101;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public int MinPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public int TotalCopies { get; private set; }
    public int Available { get; private set; }
    public int DamagedCopies { get; private set; }

    private readonly List<Review> reviews;
    public IReadOnlyList<Review> Reviews => reviews;

    public BoardGame(string name, int minPlayers, int maxPlayers, int totalCopies = 1)
    {
        Id = NextId++;
        Name = name.Trim();
        MinPlayers = Math.Max(1, minPlayers);
        MaxPlayers = Math.Max(MinPlayers, maxPlayers);
        TotalCopies = Math.Max(1, totalCopies);
        Available = TotalCopies;
        DamagedCopies = 0;
        reviews = new List<Review>();
    }

    public BoardGame(string name) : this(name, 2, 4, 1) { }

    public bool TakeCopy()
    {
        if (Available <= 0)
            return false;

        Available -= 1;
        return true;
    }

    public bool ReturnCopy(bool isIntact)
    {
        int issuedCopies = TotalCopies - Available - DamagedCopies;
        if (issuedCopies <= 0)
            return false;

        if (isIntact)
            Available += 1;
        else
            DamagedCopies += 1;

        return true;
    }

    public void AddReview(Review review)
    {
        if (review != null)
            reviews.Add(review);
    }

    public double GetAverageRating()
    {
        if (reviews.Count == 0)
            return 0;

        double sum = 0;
        foreach (var r in reviews)
        {
            sum += r.Rating;
        }
        return Math.Round(sum / reviews.Count, 1);
    }

    public string GetReviewsSummary()
    {
        if (reviews.Count == 0)
            return "—";

        string avg = GetAverageRating().ToString("0.0", CultureInfo.InvariantCulture);
        return $"{reviews.Count} отз., ср. {avg}";
    }
}