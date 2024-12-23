using IMDbMovieAnalytics.API.Models;

namespace IMDbMovieAnalytics.API.Services
{
    public class MovieAnalyticsService : IMovieAnalyticsService
    {
        private const long BlockbusterThreshold = 100_000_000;

        public MovieAnalytics CalculateAnalytics(MovieAnalyticsDTO movie)
        {
            var ratingMetrics = CalculateRatingMetrics(movie);
            var genreDistribution = CalculateGenreDistribution(movie);
            var awardMetrics = ExtractAwardMetrics(movie);

            return new MovieAnalytics(movie.Title,
                                      movie.Plot,
                                      movie.Year,
                                      ratingMetrics,
                                      genreDistribution,
                                      awardMetrics);
        }

        private RatingAnalytics CalculateRatingMetrics(MovieAnalyticsDTO movie)
        {
            var ratingCategory = movie.ImDbRating switch
            {
                >= 8.0m => "Outstanding",
                >= 7.0m => "Very Good",
                >= 6.0m => "Good",
                >= 5.0m => "Average",
                _ => "Below Average"
            };

            var performanceMetric = (movie.ImDbRating, movie.BoxOffice) switch
            {
                ( >= 7.0m, >= BlockbusterThreshold) => "Critical and Commercial Success",
                ( >= 7.0m, _) => "Critical Success",
                (_, >= BlockbusterThreshold) => "Commercial Success",
                _ => "Limited Impact"
            };

            return new RatingAnalytics(
                AverageRating: movie.ImDbRating,
                RatingCategory: ratingCategory,
                PerformanceMetric: performanceMetric,
                IsBlockbuster: movie.BoxOffice >= BlockbusterThreshold
            );
        }

        private Dictionary<string, int> CalculateGenreDistribution(MovieAnalyticsDTO movie) =>
            movie.GenreList
                .GroupBy(g => g)
                .ToDictionary(g => g.Key, g => g.Count());

        private AwardsAnalytics ExtractAwardMetrics(MovieAnalyticsDTO movie) =>
            new(movie.PrestigiousAwardWins, movie.TotalAwardWins, movie.TotalAwardNominations);
    }
}
