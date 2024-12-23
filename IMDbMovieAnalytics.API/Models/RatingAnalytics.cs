namespace IMDbMovieAnalytics.API.Models
{
    public record RatingAnalytics(
        decimal AverageRating,
        string RatingCategory,
        string PerformanceMetric,
        bool IsBlockbuster
    );
}
