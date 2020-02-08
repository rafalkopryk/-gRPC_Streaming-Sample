using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
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

                var delay = new Duration()
                {
                    Seconds = 1
                };

                #region Server streaming call

                var weatherForecastRequest = new WeatherForecastRequest
                {
                    NumberOfDays = 30,
                    Delay = delay
                };

                using var weatherForecastStreamCall = _weatherForecastClient
                    .GetWeatherForecastStream(weatherForecastRequest);

                await foreach (var weatherForecast in weatherForecastStreamCall.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    var date = weatherForecast.Date.ToDateTime();
                    await Console.Out.WriteLineAsync($"Date: {date:dd-MM-yyyy} | Temparature: {weatherForecast.TemperatureC,3} | Summary: {weatherForecast.Summary}");
                }

                #endregion

                #region Client streaming call

                //var getWeatherForecastsByDateQuerires = Enumerable
                //    .Range(1, 5)
                //    .Select(index => new WeatherForecastByDateRequest
                //    {
                //        Date = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(index)),
                //        Delay = delay
                //    });

                //using var weatherForecastStreamCall = _weatherForecastClient
                //    .GetWeatherForecastByDate();

                //foreach (var request in getWeatherForecastsByDateQuerires)
                //{
                //    await weatherForecastStreamCall.RequestStream.WriteAsync(request);
                //}

                //await weatherForecastStreamCall.RequestStream.CompleteAsync();
                //var results = await weatherForecastStreamCall;

                //foreach (var weatherForecast in results.WeatherForecasts)
                //{
                //    var date = weatherForecast.Date.ToDateTime();
                //    await Console.Out.WriteLineAsync($"Date: {date:dd-MM-yyyy} | Temparature: {weatherForecast.TemperatureC,3} | Summary: {weatherForecast.Summary}");
                //}

                #endregion


                #region Bi-directional streaming call

                //var getWeatherForecastsByDateQuerires = Enumerable
                //    .Range(1, 5)
                //    .Select(index => new WeatherForecastByDateRequest
                //    {
                //        Date = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(index)),
                //        Delay = delay
                //    });

                //using var weatherForecastStreamCall = _weatherForecastClient
                //    .GetWeatherForecastByDateStream();

                //var readTask = Task.Run(async () =>
                //{
                //    await foreach (var weatherForecast in weatherForecastStreamCall.ResponseStream.ReadAllAsync(stoppingToken))
                //    {
                //        var date = weatherForecast.Date.ToDateTime();
                //        await Console.Out.WriteLineAsync($"Date: {date:dd-MM-yyyy} | Temparature: {weatherForecast.TemperatureC,3} | Summary: {weatherForecast.Summary}");
                //    }
                //});

                //foreach (var request in getWeatherForecastsByDateQuerires)
                //{
                //    await weatherForecastStreamCall.RequestStream.WriteAsync(request);
                //}

                //await weatherForecastStreamCall.RequestStream.CompleteAsync();
                //await readTask;

                #endregion

            }
        }
    }
}
