namespace IMDbMovieAnalytics.API.Models
{
    public record MovieAnalytics(
        string Title,
        string Plot,
        int Year,
        RatingAnalytics RatingMetrics,
        Dictionary<string, int> GenreDistribution,
        AwardsAnalytics AwardMetrics
    );
}
