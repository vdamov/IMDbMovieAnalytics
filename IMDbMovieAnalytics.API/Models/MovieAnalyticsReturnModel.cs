namespace IMDbMovieAnalytics.API.Models
{
    public record MovieAnalyticsReturnModel(
        string Title,
        string Plot,
        int Year,
        RatingAnalytics RatingMetrics,
        Dictionary<string, int> GenreDistribution,
        AwardsAnalytics AwardMetrics
    );
}
