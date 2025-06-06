using System.Text;
using Celarix.ReceiptPrinter;
using Celarix.ReceiptPrinter.Extensions;
using Celarix.ReceiptPrinter.Sources;
using ESC_POS_USB_NET.Printer;
using NodaTime;

EncodingProvider provider = CodePagesEncodingProvider.Instance;
Encoding.RegisterProvider(provider);

var printer = new Printer("Epson TM-T20III Receipt");

var countdownSource = new CountdownSource(SystemClock.Instance);
var countdownText = countdownSource.GetCountdowns(16).ReplaceLineEndings().Split(Environment.NewLine);

foreach (var line in countdownText)
{
    printer.Append(line);
}

var multiDateText = MultiDatePrinter.GetMultiDates(DateOnly.FromDateTime(DateTimeOffset.Now.Date));
foreach (var line in multiDateText.ReplaceLineEndings().Split(Environment.NewLine))
{
    printer.Append(line);
}

//for (int i = 0; i < 10; i++)
//{
//    printer.Append("Hello World");
//}

printer.FullPaperCut();
printer.PrintDocument();