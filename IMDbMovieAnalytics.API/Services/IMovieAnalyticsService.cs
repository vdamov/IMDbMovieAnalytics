using IMDbMovieAnalytics.API.Models;

namespace IMDbMovieAnalytics.API.Services
{
    public interface IMovieAnalyticsService
    {
        MovieAnalyticsReturnModel CalculateAnalytics(MovieAnalytics movie);
    }
}
