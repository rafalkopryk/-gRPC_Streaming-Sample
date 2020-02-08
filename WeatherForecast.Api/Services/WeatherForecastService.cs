using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherForecast.Api.Services
{
    public class WeatherForecastService : Weather.WeatherBase
    {
        private readonly ILogger _logger;
        private readonly IWeatherForecastGenerator _weatherForecastGenerator;

        public WeatherForecastService(ILogger<WeatherForecastService> logger, IWeatherForecastGenerator weatherForecastGenerator)
        {
            _logger = logger;
            _weatherForecastGenerator = weatherForecastGenerator;
        }

        public async override Task GetWeatherForecastStream(WeatherForecastRequest request, IServerStreamWriter<WeatherForecast> responseStream, ServerCallContext context)
        {
            var getWeatherForecastsQuery = Enumerable
                .Range(1, request.NumberOfDays)
                .Select(index =>
                {
                    var date = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(index));
                    return _weatherForecastGenerator.GenerateWeatherForecast(Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(index)));
                });

            foreach (var day in getWeatherForecastsQuery)
            {
                if (context.CancellationToken.IsCancellationRequested) break;

                await Task.Delay(request.Delay.ToTimeSpan());

                await responseStream.WriteAsync(day);

                _logger.LogInformation($"weather forecast sent on {day.Date:dd:MM:yyyy}");
            }
        }

        public async override Task<GetWeatherForecastByDateReply> GetWeatherForecastByDate(IAsyncStreamReader<WeatherForecastByDateRequest> requestStream, ServerCallContext context)
        {
            var weatherForecasts = new List<WeatherForecast>();
            await foreach (var request in requestStream.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested) break;

                await Task.Delay(request.Delay.ToTimeSpan());

                var weatherForecast = _weatherForecastGenerator.GenerateWeatherForecast(request.Date);
                weatherForecasts.Add(weatherForecast);
            }

            return await Task.FromResult(new GetWeatherForecastByDateReply
            {
                WeatherForecasts = { weatherForecasts }
            });
        }

        public async override Task GetWeatherForecastByDateStream(IAsyncStreamReader<WeatherForecastByDateRequest> requestStream, IServerStreamWriter<WeatherForecast> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested) break;

                await Task.Delay(request.Delay.ToTimeSpan());

                var response = _weatherForecastGenerator.GenerateWeatherForecast(request.Date);
                await responseStream.WriteAsync(response);

                _logger.LogInformation($"weather forecast sent on {request.Date:dd:MM:yyyy}");
            }
        }
    }
}
