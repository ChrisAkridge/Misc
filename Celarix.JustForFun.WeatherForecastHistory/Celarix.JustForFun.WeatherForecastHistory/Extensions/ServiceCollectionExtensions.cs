using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Celarix.JustForFun.WeatherForecastHistory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Celarix.JustForFun.WeatherForecastHistory.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddWeatherForecastHistory(this IServiceCollection services, string connectionString)
		{
			services.AddDbContext<HistoryContext>(options => options.UseSqlite(connectionString));
			services.AddScoped(_ =>
			{
				return new MapperConfiguration(cfg =>
				{
					cfg.CreateMap<Data.Model.Forecast, Model.Forecast>().ReverseMap();
					cfg.CreateMap<Data.Model.ForecastDay, Model.ForecastDay>().ReverseMap();
					cfg.CreateMap<Data.Model.ForecastSource, Model.ForecastSource>().ReverseMap();
					cfg.CreateMap<Data.Model.Observation, Model.Observation>().ReverseMap();
				});
			});
		}
	}
}
