using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherForecast.Api;
using static WeatherForecast.Api.Weather;

namespace WeatherForecast.Client
{
    public class WeatherForecast : BackgroundService
    {
        private readonly ILogger<WeatherForecast> _logger;
        private readonly WeatherClient _weatherForecastClient;

        public WeatherForecast(ILogger<WeatherForecast> logger, WeatherClient weatherClient)
        {
            _logger = logger;
            _weatherForecastClient = weatherClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

                var weatherForecastRequest = new WeatherForecastRequest
                {
                    NumberOfDays = 30
                };

                using var weatherForecastStreamCall = _weatherForecastClient
                    .GetWeatherForecastStream(weatherForecastRequest);

                await foreach (var weatherForecast in weatherForecastStreamCall.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    var date = weatherForecast.Date.ToDateTime();
                    await Console.Out.WriteLineAsync($"Date: {date:dd-MM-yyyy} | Temparature: {weatherForecast.TemperatureC,3} | Summary: {weatherForecast.Summary}");
                }
            }
        }
    }
}
