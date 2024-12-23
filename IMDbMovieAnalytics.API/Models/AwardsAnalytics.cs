namespace IMDbMovieAnalytics.API.Models
{
    public record AwardsAnalytics(
        int PrestigiousAwardWin,
        int TotalAwardWins,
        int TotalAwardNominations
    );
}
