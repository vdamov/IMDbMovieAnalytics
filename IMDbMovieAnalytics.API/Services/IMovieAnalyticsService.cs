using IMDbMovieAnalytics.API.Models;

namespace IMDbMovieAnalytics.API.Services
{
    public interface IMovieAnalyticsService
    {
        MovieAnalytics CalculateAnalytics(MovieAnalyticsDTO movie);
    }
}
