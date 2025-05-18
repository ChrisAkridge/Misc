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

printer.FullPaperCut();
printer.PrintDocument();