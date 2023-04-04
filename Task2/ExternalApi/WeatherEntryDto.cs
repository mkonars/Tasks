using System.Text.Json.Serialization;

namespace Task2.ExternalApi
{
    public class WeatherEntryDto
    {

        public class LocationDto
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = null!;

            [JsonPropertyName("country")]
            public string Country { get; set; } = null!;
        }

        public class CurrentDto
        {
            [JsonPropertyName("wind_kph")]
            public float WindSpeed { get; set; }

            [JsonPropertyName("temp_c")]
            public float Temperature { get; set; }

            [JsonPropertyName("last_updated_epoch")]
            public long LastUpdatedEpoch { get; set; }
        }

        [JsonPropertyName("location")]
        public LocationDto Location { get; set; } = null!;

        [JsonPropertyName("current")]
        public CurrentDto Current { get; set; } = null!;
    }


}
