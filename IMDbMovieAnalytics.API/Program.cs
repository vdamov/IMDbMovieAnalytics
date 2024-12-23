using IMDbMovieAnalytics.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IMovieApiClient, MovieApiClient>();
builder.Services.AddScoped<IMovieAnalyticsService, MovieAnalyticsService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// API endpoints
app.MapGet("/api/movie/search/{query}", async (
    string query,
    IMovieApiClient client,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Searching for movie with query: {Query}", query);
    var result = await client.SearchMovieAsync(query);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("SearchMovie")
.WithOpenApi();

app.MapGet("/api/movie/analytics/{id}", async (
    string id,
    IMovieApiClient client,
    IMovieAnalyticsService analyticsService,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Calculating analytics for movie ID: {Id}", id);

    var movie = await client.GetMovieDetailsAsync(id);
    if (movie is null)
        return Results.NotFound();

    var analytics = analyticsService.CalculateAnalytics(movie);
    return Results.Ok(analytics);
})
.WithName("GetMovieAnalytics")
.WithOpenApi();

app.Run();