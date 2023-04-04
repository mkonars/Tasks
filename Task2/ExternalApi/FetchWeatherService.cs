using System.Text.Json;
using Task2.Database;
using Task2.Domain;

namespace Task2.ExternalApi
{

    public class FetchWeatherService : BackgroundService
    {
        private readonly WeatherDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        private readonly List<string> _cities = new List<string> { "Paris", "London", "Berlin", "Madrid", "Warsaw" };

        public FetchWeatherService(WeatherDbContext dbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["WeatherApiKey"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchAndSave();
                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task FetchAndSave()
        {
            var httpClient = _httpClientFactory.CreateClient();

            foreach (var city in _cities)
            {
                await FetchCityData(httpClient, city);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task FetchCityData(HttpClient httpClient, string city)
        {
            var response = await httpClient.GetAsync($"http://api.weatherapi.com/v1/current.json?key={_apiKey}&q={city}&aqi=no");

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<WeatherEntryDto>(json);

            var entry = new WeatherEntry
            {
                City= city,
                Country = dto!.Location.Country,
                UpdateTime = DateTimeOffset.FromUnixTimeSeconds(dto.Current.LastUpdatedEpoch).DateTime,
                WindSpeed = dto.Current.WindSpeed,
                Temperature = dto.Current.Temperature
            };

            _dbContext.WeatherEntries.Add(entry);
        }
    }
}
