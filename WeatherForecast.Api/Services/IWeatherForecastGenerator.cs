using Google.Protobuf.WellKnownTypes;

namespace WeatherForecast.Api.Services
{
    public interface IWeatherForecastGenerator
    {
        WeatherForecast GenerateWeatherForecast(Timestamp date);
    }
}
