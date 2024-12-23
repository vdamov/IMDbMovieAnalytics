namespace IMDbMovieAnalytics.API.Models
{
    public record SearchMovieDTO(
        string Id,
        string Title,
        int Year,
        string Stars
    );
}
