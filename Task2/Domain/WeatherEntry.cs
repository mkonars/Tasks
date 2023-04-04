namespace Task2.Domain
{
    public class WeatherEntry
    {
        public int Id { get; set; }

        public string Country { get; set; } = null!;

        public string City { get; set; } = null!;

        public float WindSpeed { get; set; }

        public float Temperature { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
