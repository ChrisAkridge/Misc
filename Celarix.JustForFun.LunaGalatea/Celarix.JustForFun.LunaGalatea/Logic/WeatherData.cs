using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Exceptions;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Forecast;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.HourlyForecast;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Points;
using Newtonsoft.Json;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic
{
    internal sealed class WeatherData
    {
        private const double LouisvilleLatitude = 38.25d;
        private const double LouisvilleLongitude = -85.75d;
        // Bowman Field
        private const string LouisvilleObservationURL = "https://api.weather.gov/stations/KLOU/observations";

        private readonly HttpClient httpClient;
        private readonly DateTimeOffset requestCycleStart;
        private readonly TimeSpan currentEasternTimeOffset;

        public WeatherData()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Chris Akridge (cakridge2@gmail.com) for simple weather widget");
            requestCycleStart = DateTimeOffset.Now;

            var offset = DateTimeZoneProviders.Tzdb["America/New_York"]
                .GetUtcOffset(SystemClock.Instance.GetCurrentInstant());
            currentEasternTimeOffset = TimeSpan.FromMilliseconds(offset.Milliseconds);
        }

        public async Task<WeatherResponse> GetPartialWeatherResponse(DateTimeOffset lastFullRequestTime) => await GetWeatherResponse(WeatherResponseType.ObservationOnly, lastFullRequestTime);

        public async Task<WeatherResponse> GetFullWeatherResponse() =>
            await GetWeatherResponse(WeatherResponseType.ObservationAndForecasts, DateTimeOffset.Now);
        
        private async Task<WeatherResponse> GetWeatherResponse(WeatherResponseType responseType, DateTimeOffset lastFullRequestTime)
        {
            var weatherResponse = new WeatherResponse
            {
                RequestCycleStart = requestCycleStart,
                RequestTime = DateTimeOffset.Now,
                NextPlannedRequestTime = DateTimeOffset.Now.AddMinutes(15d),
                NextPlannedFullRequestTime = lastFullRequestTime.AddHours(4d),
                WeatherResponseType = responseType
            };
            PointsResponse points;

            try
            {
                points = await GetPoints()
                    ?? throw new WebResponseNullException("Failed to get points response");
            }
            catch (Exception ex)
            {
                weatherResponse.ThrownException = ex;
                return weatherResponse;
            }

            try
            {
                var observation = await GetObservation()
                    ?? throw new WebResponseNullException("Failed to get observation");
                weatherResponse.Observation = GetObservationFromNOAAObservation(observation)
                    ?? throw new InvalidOperationException("Observation request was successful but no observations were present");
            }
            catch (Exception ex)
            {
                weatherResponse.ThrownException = ex;
                return weatherResponse;
            }

            if (responseType == WeatherResponseType.ObservationAndForecasts)
            {
                try
                {
                    var forecastURL = points.properties.forecast;
                    var forecast = await GetForecast(forecastURL)
                        ?? throw new WebResponseNullException("Failed to get forecast response");
                    weatherResponse.PeriodForecasts = forecast.properties.periods
                        .Select(p => GetForecastPeriodFromNOAAForecastPeriod(p)
                            ?? throw new InvalidOperationException("Period object was null"))
                        .ToList();
                }
                catch (Exception ex)
                {
                    weatherResponse.ThrownException = ex;
                    return weatherResponse;
                }

                try
                {
                    var hourlyForecastURL = points.properties.forecastHourly;
                    var hourlyForecast = await GetHourlyForecast(hourlyForecastURL)
                        ?? throw new WebResponseNullException("Failed to get hourly forecast response");
                    weatherResponse.HourlyForecasts = hourlyForecast.properties.periods
                        .Select(p => GetForecastPeriodFromNOAAHourlyForecastPeriod(p)
                            ?? throw new InvalidOperationException("Period object was null"))
                        .ToList();
                }
                catch (Exception ex)
                {
                    weatherResponse.ThrownException = ex;
                    return weatherResponse;
                }
            }

            weatherResponse.RequestSuccessful = true;
            return weatherResponse;
        }

        /// <summary>
        /// Runs the first step, which gets a list of second-step URLs to access resources for this
        /// location.
        /// </summary>
        /// <returns>A <see cref="PointsResponse"/> object containing second-step URLs.</returns>
        private async Task<PointsResponse?> GetPoints()
        {
            var url = $"https://api.weather.gov/points/{LouisvilleLatitude:F2},{LouisvilleLongitude:F2}";
            var pointsJson = await httpClient.GetStringAsync(url);

            return JsonConvert.DeserializeObject<PointsResponse>(pointsJson);
        }

        /// <summary>
        /// Retrieves a period forecast (i.e. tonight, tomorrow, tomorrow night, etc.) from a URL
        /// specified in a <see cref="PointsResponse"/> object.
        /// </summary>
        /// <param name="forecastURL">The URL to get the forecast data from.</param>
        /// <returns>A <see cref="ForecastResponse"/> containing a period forecast.</returns>
        private async Task<ForecastResponse?> GetForecast(string forecastURL)
        {
            var forecastJson = await httpClient.GetStringAsync(forecastURL);

            return JsonConvert.DeserializeObject<ForecastResponse>(forecastJson);
        }

        private async Task<HourlyForecastResponse?> GetHourlyForecast(string hourlyForecastURL)
        {
            var hourlyForecastJson = await httpClient.GetStringAsync(hourlyForecastURL);

            return JsonConvert.DeserializeObject<HourlyForecastResponse>(hourlyForecastJson);
        }

        private async Task<ObservationResponse?> GetObservation()
        {
            var observationJson = await httpClient.GetStringAsync(LouisvilleObservationURL);

            return JsonConvert.DeserializeObject<ObservationResponse>(observationJson);
        }

        private Observation? GetObservationFromNOAAObservation(ObservationResponse noaaObservation)
        {
            var mostRecentObservation = noaaObservation.features.MaxBy(f => f.properties.timestamp)?.properties;

            if (mostRecentObservation == null)
            {
                return null;
            }
            
            return new Observation
            {
                Timestamp = new DateTimeOffset(mostRecentObservation.timestamp, currentEasternTimeOffset),
                TextDescription = mostRecentObservation.textDescription,
                Temperature = GetDegreeCelsiusFromNullableFloat(mostRecentObservation.temperature.value),
                DewPoint = GetDegreeCelsiusFromNullableFloat(mostRecentObservation.dewpoint.value),
                WindSpeed = GetKilometerPerHourFromNullableFloat(mostRecentObservation.windSpeed.value),
                WindGust = GetKilometerPerHourFromNullableFloat(mostRecentObservation.windGust.value),
                Visibility = new Meter(mostRecentObservation.visibility.value),
                PrecipitationLast6Hours = GetMillimeterFromNullableInt(mostRecentObservation.precipitationLast6Hours.value),
                RelativeHumidity = mostRecentObservation.relativeHumidity.value,
                BarometricPressure = GetPascalFromNullableFloat(mostRecentObservation.barometricPressure.value),
                WindChill = GetDegreeCelsiusFromNullableFloat(mostRecentObservation.windChill.value),
                HeatIndex = GetDegreeCelsiusFromNullableFloat(mostRecentObservation.heatIndex.value)
            };
        }

        private ForecastPeriod? GetForecastPeriodFromNOAAHourlyForecastPeriod(NOAAWeatherModels.HourlyForecast.Period? noaaPeriod)
        {
            if (noaaPeriod == null) { return null; }

            return new ForecastPeriod
            {
                PeriodName = noaaPeriod.name,
                StartTime = noaaPeriod.startTime,
                EndTime = noaaPeriod.endTime,
                TemperatureF = noaaPeriod.temperature,
                PrecipitationChance = noaaPeriod.probabilityOfPrecipitation.value,
                RelativeHumidity = noaaPeriod.relativeHumidity.value,
                WindSpeedMPH = double.Parse(noaaPeriod.windSpeed.Split(' ')[0]),
                ShortForecast = noaaPeriod.shortForecast
            };
        }

        private ForecastPeriod? GetForecastPeriodFromNOAAForecastPeriod(NOAAWeatherModels.Forecast.Period? noaaPeriod)
        {
            if (noaaPeriod == null) { return null; }

            return new ForecastPeriod
            {
                PeriodName = noaaPeriod.name,
                StartTime = noaaPeriod.startTime,
                EndTime = noaaPeriod.endTime,
                TemperatureF = noaaPeriod.temperature,
                PrecipitationChance = noaaPeriod.probabilityOfPrecipitation.value,
                RelativeHumidity = noaaPeriod.relativeHumidity.value,
                WindSpeedMPH = double.Parse(noaaPeriod.windSpeed.Split(' ')[0]),
                ShortForecast = noaaPeriod.shortForecast,
                DetailedForecast = noaaPeriod.detailedForecast
            };
        }

        private DegreeCelsius? GetDegreeCelsiusFromNullableFloat(float? value) =>
            value == null
                ? null
                : new DegreeCelsius(value.Value);

        private KilometerPerHour? GetKilometerPerHourFromNullableFloat(float? value) =>
            value == null
                ? null
                : new KilometerPerHour(value.Value);

        private Millimeter? GetMillimeterFromNullableInt(int? value) =>
            value == null
                ? null
                : new Millimeter(value.Value);
        
        private Pascal? GetPascalFromNullableFloat(float? value) =>
            value == null
                ? null
                : new Pascal(value.Value);
    }
}
