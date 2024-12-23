namespace IMDbMovieAnalytics.API.Models
{
    public record SearchMovieReturnModel(
        string Id,
        string Title,
        int Year,
        string Stars
    );
}
