using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class WeatherProvider : IAsyncProvider<string>
    {
        private const int FullRequestDelay = 4 * 60 * 60;
        private const int PartialRequestDelay = 15 * 60;
        
        private readonly WeatherData weatherData = new WeatherData();
        private WeatherResponse lastFullWeatherResponse;
        
        public bool UseMonospaceFont => false;
        public Task<string> GetDisplayObject() => throw new InvalidOperationException("I really should have architected this better");

        public async Task<TryGetWeatherDisplayObjectResult> TryGetWeatherResponse(int timerTicks)
        {
            if (timerTicks % PartialRequestDelay != 0)
            {
                return new TryGetWeatherDisplayObjectResult
                {
                    DisplayObject = null,
                    Success = false
                };
            }
            
            const int bufferSize = 65536;
            await using var fileStream = File.Open(GetWeatherDataStreamPath(), FileMode.Append, FileAccess.Write);
            await using var writer =
                new StreamWriter(fileStream,
                    Encoding.UTF8, bufferSize);
            
            if (timerTicks % FullRequestDelay == 0)
            {
                lastFullWeatherResponse = await weatherData.GetFullWeatherResponse();
                WriteFullRequestToStream(writer, lastFullWeatherResponse);
            }
            else
            {
                var partialResponse = await weatherData.GetPartialWeatherResponse(lastFullWeatherResponse.RequestTime);
                partialResponse.HourlyForecasts = lastFullWeatherResponse.HourlyForecasts;
                partialResponse.PeriodForecasts = lastFullWeatherResponse.PeriodForecasts;
                WritePartialRequestToStream(writer, partialResponse);
                lastFullWeatherResponse = partialResponse;
            }

            return new TryGetWeatherDisplayObjectResult
            {
                DisplayObject = GetDisplayObjectFromWeatherResponse(lastFullWeatherResponse),
                Success = true
            };
        }

        private string GetDisplayObjectFromWeatherResponse(WeatherResponse response)
        {
            var observation = response.Observation;

            var timestamp = $"Report from {response.Observation.Timestamp:hh:mm tt}";
            var description = !string.IsNullOrEmpty(observation.TextDescription)
                ? observation.TextDescription
                : "Description N/A";
            var currentTemperature = observation.Temperature != null
                ? $"{observation.Temperature.Value.Fahrenheit():F0}\u00b0F"
                : "N/A";
            var feelsLike = observation.WindChill.HasValue
                ? $"(feels like {observation.WindChill.Value.Fahrenheit():F0}\u00b0F)"
                : observation.HeatIndex.HasValue
                    ? $"(feels like {observation.HeatIndex.Value.Fahrenheit():F0}\u00b0F)"
                    : "";
            var currentHumidity = observation.RelativeHumidity != null
                ? $"relative humidity {observation.RelativeHumidity.Value:F0}%"
                : "relative humidity N/A";
            var currentWindSpeed = observation.WindSpeed != null
                ? $"winds {observation.WindSpeed.Value.MilesPerHour():F0} MPH"
                : "winds N/A";
            var currentWindGust = observation.WindGust != null
                ? $"(gust {observation.WindGust.Value.MilesPerHour():F0} MPH)"
                : "";
            var currentPressure = observation.BarometricPressure != null
                ? $"pressure {observation.BarometricPressure.Value.MillimetersOfMercury():F0} mmHg"
                : "pressure N/A";

            var currentConditions =
                $"{timestamp}, {description}, {currentTemperature}, {feelsLike}, {currentHumidity}, {currentWindSpeed} {currentWindGust}, {currentPressure}";

            var nextPeriodForecast = response.PeriodForecasts.MinBy(f => f.StartTime);
            var detailedForecast = !string.IsNullOrEmpty(nextPeriodForecast!.DetailedForecast)
                ? nextPeriodForecast.DetailedForecast
                : "description N/A";
            var periodForecastTemperature = $"{nextPeriodForecast.TemperatureF:F0}\u00b0F";
            var periodPrecipitationChance = nextPeriodForecast.PrecipitationChance != null
                ? $"{nextPeriodForecast.PrecipitationChance:F0}% chance of precip"
                : "0% chance of precip";
            var periodHumidity = nextPeriodForecast.RelativeHumidity != null
                ? $"{nextPeriodForecast.RelativeHumidity:F0}% humidity"
                : "N/A humidity";
            var periodWindSpeed = $"winds {nextPeriodForecast.WindSpeedMPH} MPH";
            var nextPeriodForecastString =
                $"{nextPeriodForecast.PeriodName}: {detailedForecast}, {periodForecastTemperature}, {periodPrecipitationChance}, {periodHumidity}, {periodWindSpeed}";

            var hourlyForecasts = string.Join(", ", response.HourlyForecasts
                .OrderBy(f => f.StartTime)
                .Take(8)
                .Select(f => $"{FormatDateTimeOffsetHourOfDayOnly(f.StartTime)}: {f.TemperatureF}\u00b0F"));

            var nextForecastIn =
                $"Next observation update at {response.NextPlannedRequestTime:hh:mm tt}, next forecast update at {response.NextPlannedFullRequestTime:hh:mm tt}.";

            return string.Join(Environment.NewLine,
                currentConditions,
                nextPeriodForecastString,
                hourlyForecasts,
                nextForecastIn);
        }

        private string GetWeatherDataStreamPath()
        {
            var baseFolderPath = Environment.MachineName.ToLowerInvariant() switch
            {
                "pavilion-core" => @"F:\Documents\Files\Pictures\Miscellaneous\TRIMARC",
                "akridge-pc" => @"C:\Users\ChrisAckridge\Pictures\TRIMARC",
                "bluebell01" => @"C:\Users\celarix\Pictures\TRIMARC",
                _ => throw new ArgumentOutOfRangeException()
            };

            return Path.Combine(baseFolderPath, "noaaWeatherData.txt");
        }

        private void WritePartialRequestToStream(StreamWriter writer, WeatherResponse partialResponse)
        {
            writer.WriteLine("Begin Partial Weather Response");
            
            writer.WriteLine($"Request made {FormatDateTimeOffset(partialResponse.RequestTime)}");

            if (!partialResponse.RequestSuccessful)
            {
                writer.WriteLine($"Request failed with {partialResponse.ThrownException!.GetType().Name}");
                writer.WriteLine($"Message: {partialResponse.ThrownException.Message}");
                writer.WriteLine("End Partial Weather Response");
                return;
            }
            
            WriteObservationToStream(writer, partialResponse.Observation);
            
            writer.WriteLine("End Partial Weather Response");
        }

        private void WriteFullRequestToStream(StreamWriter writer, WeatherResponse fullResponse)
        {
            writer.WriteLine("Begin Full Weather Response");

            writer.WriteLine($"Request made {FormatDateTimeOffset(fullResponse.RequestTime)}");

            if (!fullResponse.RequestSuccessful)
            {
                writer.WriteLine($"Request failed with {fullResponse.ThrownException!.GetType().Name}");
                writer.WriteLine($"Message: {fullResponse.ThrownException.Message}");
                writer.WriteLine("End Partial Weather Response");
                return;
            }

            WriteObservationToStream(writer, fullResponse.Observation);

            writer.WriteLine("Begin Period Forecasts");
            for (int i = 0; i < fullResponse.PeriodForecasts.Count; i++)
            {
                WriteForecastToStream(writer, fullResponse.PeriodForecasts[i], i, fullResponse.PeriodForecasts.Count);
            }
            writer.WriteLine("End Period Forecasts");

            writer.WriteLine("Begin Hourly Forecasts");
            for (int i = 0; i < fullResponse.HourlyForecasts.Count; i++)
            {
                WriteForecastToStream(writer, fullResponse.HourlyForecasts[i], i, fullResponse.HourlyForecasts.Count);
            }
            writer.WriteLine("End Hourly Forecasts");

            writer.WriteLine("End Full Weather Response");
        }

        private void WriteObservationToStream(StreamWriter writer, Observation observation)
        {
            writer.WriteLine("Begin Observation");
            
            writer.WriteLine($"Description: {observation.TextDescription}");
            writer.WriteLine($"Temperature: {observation.Temperature?.Fahrenheit().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Dew Point: {observation.DewPoint?.Fahrenheit().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Wind Speed: {observation.WindSpeed?.MilesPerHour().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Wind Gust: {observation.WindGust?.MilesPerHour().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Visiblity: {observation.Visibility?.Feet().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Precipitation Last 6 Hours: {observation.PrecipitationLast6Hours?.Inches().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Barometric Pressure: {observation.BarometricPressure?.MillimetersOfMercury().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Wind Chill: {observation.WindChill?.Fahrenheit().ToString("F0") ?? "N/A"}");
            writer.WriteLine($"Heat Index: {observation.HeatIndex?.Fahrenheit().ToString("F0") ?? "N/A"}");
            
            writer.WriteLine("End Observation");
        }

        private void WriteForecastToStream(StreamWriter writer,
            ForecastPeriod forecast,
            int periodIndex,
            int periodCount)
        {
            writer.WriteLine($"Begin Forecast Period {periodIndex + 1} of {periodCount}");

            if (!string.IsNullOrEmpty(forecast.PeriodName))
            {
                writer.WriteLine($"For: {forecast.PeriodName}");
            }
            
            writer.WriteLine($"From: {FormatDateTimeOffset(forecast.StartTime)}");
            writer.WriteLine($"To: {FormatDateTimeOffset(forecast.EndTime)}");
            writer.WriteLine($"Temperature: {forecast.TemperatureF:F0}");
            writer.WriteLine($"Precipitation Chance: {(forecast.PrecipitationChance.HasValue ? forecast.PrecipitationChance.Value.ToString("F0") : "N/A")}");
            writer.WriteLine($"Wind Speed: {forecast.WindSpeedMPH}");
            writer.WriteLine($"Short Forecast: {forecast.ShortForecast}");
            
            writer.WriteLine($"End Forecast Period {periodIndex + 1} of {periodCount}");
        }

        private string FormatDateTimeOffset(DateTimeOffset dateTimeOffset) =>
            dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");

        private string FormatDateTimeOffsetHourOfDayOnly(DateTimeOffset dateTimeOffset) =>
            dateTimeOffset.ToString("h tt");
    }
}
