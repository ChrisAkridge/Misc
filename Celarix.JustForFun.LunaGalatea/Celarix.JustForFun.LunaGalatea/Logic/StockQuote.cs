using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LewisFam.Stocks;
using Newtonsoft.Json.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic
{
    public sealed class StockQuote
    {
        private const string CNBCBaseUrl = @"https://quote.cnbc.com/quote-html-webservice/quote.htm?exthrs=1&noform=1&fund=1&output=json&events=1&requestMethod=quick&symbols=";
        
        private static HttpClient client = new HttpClient();
        
        public string FriendlyName { get; set; }
        public string Symbol { get; set; }
        public double? CurrentPrice { get; set; }
        public double LastClose { get; set; }
        public double? PriceChange => CurrentPrice - LastClose;
        public double? PercentChange => ((CurrentPrice / LastClose) - 1d) * 100d;

        public static async Task<StockQuote> CreateFromSymbolAndFriendlyName(string symbol, string friendlyName)
        {
            var stock = await StocksUtil.FindStockAsync(symbol);

            return new StockQuote
            {
                FriendlyName = friendlyName,
                Symbol = symbol,
                CurrentPrice = stock.Price,
                LastClose = await stock.GetLastClosePrice()
            };
        }

        public static async Task<List<StockQuote>> CreateFromSymbolsAndFriendlyNames(IReadOnlyList<KeyValuePair<string, string>> symbolsAndNames)
        {
            // Fine, I'll do it myself
            var fullUri = $"{CNBCBaseUrl}{string.Join('|', symbolsAndNames.Select(kvp => kvp.Value))}";
            var quoteJson = await client.GetStringAsync(fullUri);
            var quoteJObject = JObject.Parse(quoteJson);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var quickQuote = (JArray)quoteJObject["QuickQuoteResult"]["QuickQuote"];
#pragma warning restore CS8602 // Dereference of a possibly null reference.


#pragma warning disable CS8604 // Possible null reference argument.
            return quickQuote
                .Select(q => new
                {
                    Quote = q,
                    SymbolAndName = symbolsAndNames.First(kvp => (string)q["symbol"] == kvp.Value)
                })
                .Select(qs => new StockQuote
                {
                    FriendlyName = qs.SymbolAndName.Key,
                    Symbol = qs.SymbolAndName.Value,
                    CurrentPrice = double.Parse((string)qs.Quote["last"]),
                    LastClose = double.Parse((string)qs.Quote["previous_day_closing"])
                })
                .ToList();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }
}
