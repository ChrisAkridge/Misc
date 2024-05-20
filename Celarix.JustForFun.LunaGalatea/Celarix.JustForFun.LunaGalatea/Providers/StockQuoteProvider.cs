using Celarix.JustForFun.LunaGalatea.Logic;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class StockQuoteProvider : IAsyncProvider<List<string>>
    {
        public bool UseMonospaceFont => false;
        
        public async Task<List<string>> GetDisplayObject()
        {
            var stockNamesAndSymbols = new[]
            {
                new KeyValuePair<string, string>("Dow Jones Industrial Average", ".DJI"),
                new KeyValuePair<string, string>("NASDAQ 100", ".IXIC"),
                new KeyValuePair<string, string>("Vanguard Total Stock Market", "VTI")
            };

            List<StockQuote> quotes;
            try
            {
                quotes = await StockQuote.CreateFromSymbolsAndFriendlyNames(stockNamesAndSymbols);
            }
            catch (Exception ex)
            {
                return new List<string>
                {
                    $"Failed to load stock info: {ex.Message}"
                };
            }
            
            return quotes
                .Select(q => $"{q.FriendlyName}: {q.CurrentPrice:N2} ({q.PriceChange:N2}, {q.PercentChange:F2}%)")
                .ToList();
        }
    }
}
