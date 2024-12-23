﻿namespace IMDbMovieAnalytics.API.Models
{
    public record MovieAnalyticsDTO(
        string Id,
        string Title,
        int Year,
        decimal ImDbRating,
        List<string> GenreList,
        string Plot,
        long BoxOffice,
        int PrestigiousAwardWins,
        int TotalAwardWins,
        int TotalAwardNominations
    );
}