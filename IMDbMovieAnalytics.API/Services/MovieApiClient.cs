using IMDbMovieAnalytics.API.Constants;
using IMDbMovieAnalytics.API.Models;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IMDbMovieAnalytics.API.Services
{
    public class MovieApiClient : IMovieApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MovieApiClient> _logger;

        public MovieApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MovieApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            var apiKey = _configuration["RapidApi:Key"]
                ?? throw new InvalidOperationException("RapidAPI key not configured");

            _httpClient.BaseAddress = new Uri(ApiSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", ApiSettings.RapidApiHost);
        }

        public async Task<List<SearchMovieDTO>?> SearchMovieAsync(string query)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/auto-complete?query={Uri.EscapeDataString(query)}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON
                var responseData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                var movies = new List<SearchMovieDTO>();

                // Navigate to the "d" array inside "data"
                if (responseData.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("d", out var moviesArray))
                {
                    foreach (var movieElement in moviesArray.EnumerateArray())
                    {
                        // Extract movie details with appropriate defaults
                        var id = movieElement.GetProperty("id").GetString() ?? "Unknown";
                        var title = movieElement.GetProperty("l").GetString() ?? "Untitled";
                        var year = movieElement.TryGetProperty("y", out var yearElement) && yearElement.TryGetInt32(out var parsedYear) ? parsedYear : 0;
                        var stars = movieElement.TryGetProperty("s", out var starsElement) ? starsElement.GetString() : "Unknown";

                        // Add the movie to the list (missing fields filled with placeholders)
                        movies.Add(new SearchMovieDTO(
                            id,
                            title,
                            year,
                            Stars: stars ?? "Unknown"
                        ));
                    }
                }

                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for movies with query: {Query}", query);
                return null;
            }
        }

        public async Task<MovieAnalyticsDTO?> GetMovieDetailsAsync(string tconst)
        {
            try
            {
                var overviewResponseTask = _httpClient.GetAsync($"/title/get-overview?tconst={tconst}");
                var boxOfficeResponseTask = _httpClient.GetAsync($"/title/get-box-office-summary?tconst={tconst}");
                var genresResponseTask = _httpClient.GetAsync($"/title/get-genres?tconst={tconst}");
                var awardsResponseTask = _httpClient.GetAsync($"/title/get-awards-summary?tconst={tconst}");

                var tasks = await Task.WhenAll(overviewResponseTask, boxOfficeResponseTask, genresResponseTask, awardsResponseTask);

                foreach (var result in tasks)
                {
                    result.EnsureSuccessStatusCode();
                }

                var movie = await ExtractDataFromResponses(tconst, overviewResponseTask.Result, boxOfficeResponseTask.Result, genresResponseTask.Result, awardsResponseTask.Result);

                return movie;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details for ID: {tconst}", tconst);
                return null;
            }
        }

        private static async Task<MovieAnalyticsDTO> ExtractDataFromResponses(string tconst,
                                                                              HttpResponseMessage overviewResponse,
                                                                              HttpResponseMessage boxOfficeResponse,
                                                                              HttpResponseMessage genresResponse,
                                                                              HttpResponseMessage awardsResponse)
        {
            // Extract data from overview
            var overviewBody = await overviewResponse.Content.ReadAsStringAsync();
            var overviewData = JsonSerializer.Deserialize<JsonElement>(overviewBody);

            var title = overviewData.GetProperty("data").GetProperty("title").GetProperty("titleText").GetProperty("text").GetString();
            var year = overviewData.GetProperty("data").GetProperty("title").GetProperty("releaseYear").GetProperty("year").GetInt32();
            var rating = overviewData.GetProperty("data").GetProperty("title").GetProperty("ratingsSummary").GetProperty("aggregateRating").GetDecimal();
            var plot = overviewData.GetProperty("data").GetProperty("title").GetProperty("plot").GetProperty("plotText").GetProperty("plainText").GetString();

            // Extract data from box office
            var boxOfficeBody = await boxOfficeResponse.Content.ReadAsStringAsync();
            var boxOfficeData = JsonSerializer.Deserialize<JsonElement>(boxOfficeBody);

            var boxOffice = boxOfficeData.GetProperty("data").GetProperty("title").GetProperty("worldwideGross").GetProperty("total").GetProperty("amount").GetInt64();

            // Extract data from genres
            var genresBody = await genresResponse.Content.ReadAsStringAsync();
            var genresData = JsonSerializer.Deserialize<JsonElement>(genresBody);


            var genres = genresData.GetProperty("data").GetProperty("title").GetProperty("titleGenres").GetProperty("genres").EnumerateArray()
                .Select(genre => genre.GetProperty("genre").GetProperty("text").GetString() ?? string.Empty).ToList();

            // Extract data from awards
            var awardsBody = await awardsResponse.Content.ReadAsStringAsync();
            var awardsData = JsonSerializer.Deserialize<JsonElement>(awardsBody);

            int? prestigiousAwardWins = null;
            int? totalAwardWins = null;
            int? totalAwardNominations = null;

            var titleElement = awardsData.GetProperty("data").GetProperty("title");

            if(titleElement.GetProperty("prestigiousAwardSummary").ValueKind != JsonValueKind.Null && 
                titleElement.GetProperty("prestigiousAwardSummary").GetProperty("wins").ValueKind != JsonValueKind.Null)
            {
                prestigiousAwardWins = titleElement.GetProperty("prestigiousAwardSummary").GetProperty("wins").GetInt32();
            };

            if (titleElement.GetProperty("totalWins").ValueKind != JsonValueKind.Null && 
                titleElement.GetProperty("totalWins").GetProperty("total").ValueKind != JsonValueKind.Null)
            {
                totalAwardWins = titleElement.GetProperty("totalWins").GetProperty("total").GetInt32();
            };

            if (titleElement.GetProperty("totalNominations").ValueKind != JsonValueKind.Null && 
                titleElement.GetProperty("totalNominations").GetProperty("total").ValueKind != JsonValueKind.Null)
            {
                totalAwardNominations = titleElement.GetProperty("totalNominations").GetProperty("total").GetInt32();
            };

            // Construct the Movie object
            return new MovieAnalyticsDTO(
                Id: tconst,
                Title: title ?? "Unknown",
                Year: year,
                ImDbRating: rating,
                GenreList: genres,
                Plot: plot ?? "Unknown",
                BoxOffice: boxOffice,
                PrestigiousAwardWins: prestigiousAwardWins ?? 0,
                TotalAwardNominations: totalAwardNominations ?? 0,
                TotalAwardWins: totalAwardWins ?? 0
            );
        }
    }
}
