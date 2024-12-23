using IMDbMovieAnalytics.API.Models;

namespace IMDbMovieAnalytics.API.Services
{
    public interface IMovieApiClient
    {
        Task<List<SearchMovieReturnModel>?> SearchMovieAsync(string query);
        Task<MovieAnalytics?> GetMovieDetailsAsync(string tconst);
    }
}
