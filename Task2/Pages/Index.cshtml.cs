using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Task2.Database;
using Task2.Domain;

namespace Task2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WeatherDbContext _dbContext;

        public IndexModel( WeatherDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public JsonResult OnGetLastUpdateData()
        {
            var entries = _dbContext.WeatherEntries.GroupBy(x => x.City)
             .Select(x => new WeatherEntry
             {
                 City = x.Key,
                 Temperature = x.Min(v => v.Temperature),
                 WindSpeed = x.Max(v => v.WindSpeed)
             }).ToList();

            var data = new
            {
                cities = entries.Select(x => x.City).ToList(),
                temperature = entries.Select(x => x.Temperature).ToList(),
                wind = entries.Select(x => x.WindSpeed).ToList()
            };

            return new JsonResult(data);
        }

        public JsonResult OnGetTrendData([FromQuery] string type)
        {
            var entries = _dbContext.WeatherEntries
                .Where(x => x.UpdateTime >= DateTime.UtcNow.AddHours(-2))
                .OrderBy(x => x.UpdateTime)
                .ToList()
                .GroupBy(x => x.City)
                .Select(g => new TrendData
                {
                    City = g.Key,
                    X = g.Select(i => i.UpdateTime.ToString("s")).ToList(),
                    Y = type == "wind" ? g.Select(i => i.WindSpeed).ToList() : g.Select(i => i.Temperature).ToList(),
                }
                )
                .ToList();

            return new JsonResult(entries);
        }

        class TrendData
        {
            public string City { get; set; } = null!;
            public List<string> X { get; set; } = null!;
            public List<float> Y { get; set; } = null!;
        }
    }
}