using Google.Protobuf.WellKnownTypes;
using System;

namespace WeatherForecast.Api.Services
{
    public class WeatherForecastGenerator : IWeatherForecastGenerator
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecast GenerateWeatherForecast(Timestamp date)
        {
            var rng = new Random();

            return new WeatherForecast
            {
                Date = date,
                TemperatureC = rng.Next(-10, 30),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };
        }
    }
}
