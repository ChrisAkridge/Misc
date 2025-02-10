using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal sealed class CSVReader
	{
		public T[] GetRows<T, TMap>(string csvFileText) where TMap : ClassMap
		{
			using var reader = new StringReader(csvFileText);
			using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = true,
				Delimiter = ",",
				TrimOptions = TrimOptions.Trim | TrimOptions.InsideQuotes
			});

			csv.Context.RegisterClassMap<TMap>();
			return csv.GetRecords<T>().ToArray();
		}
	}
}
