using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherForecast.Api.Services
{
    public class WeatherForecastService : Weather.WeatherBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger _logger;

        public WeatherForecastService(ILogger<WeatherForecastService> logger)
        {
            _logger = logger;
        }

        public async override Task GetWeatherForecastStream(WeatherForecastRequest request, IServerStreamWriter<WeatherForecast> responseStream, ServerCallContext context)
        {
            var rng = new Random();
            var getWeatherForecastsQuery = Enumerable.Range(1, request.NumberOfDays).Select(index => new WeatherForecast
            {
                Date = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(index)),
                TemperatureC = rng.Next(-10, 30),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });

            foreach (var day in getWeatherForecastsQuery)
            {
                if (context.CancellationToken.IsCancellationRequested) break;

                await Task.Delay(1000);

                await responseStream.WriteAsync(day);

                _logger.LogInformation($"weather forecast sent on {day.Date:dd:MM:yyyy}");
            }
        }
    }
}
