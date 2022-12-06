using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic;
using LewisFam.Stocks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class StockQuoteProvider : IAsyncProvider<List<string>>
    {
        public async Task<List<string>> GetDisplayObject()
        {
            var stockNamesAndSymbols = new[]
            {
                new KeyValuePair<string, string>("Dow Jones Industrial Average", ".DJI"),
                new KeyValuePair<string, string>("NASDAQ 100", ".IXIC"),
                new KeyValuePair<string, string>("Vanguard Total Stock Market", "VTI")
            };

            var quotes = await StockQuote.CreateFromSymbolsAndFriendlyNames(stockNamesAndSymbols);
            return quotes
                .Select(q => $"{q.FriendlyName}: {q.CurrentPrice:N2} ({q.PriceChange:N2}, {q.PercentChange:F2}%)")
                .ToList();
        }
    }
}
