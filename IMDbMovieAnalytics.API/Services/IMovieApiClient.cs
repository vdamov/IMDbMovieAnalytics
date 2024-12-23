using IMDbMovieAnalytics.API.Models;

namespace IMDbMovieAnalytics.API.Services
{
    public interface IMovieApiClient
    {
        Task<List<SearchMovieDTO>?> SearchMovieAsync(string query);
        Task<MovieAnalyticsDTO?> GetMovieDetailsAsync(string tconst);
    }
}
