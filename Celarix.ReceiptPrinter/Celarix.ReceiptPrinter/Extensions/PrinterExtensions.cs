using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.ReceiptPrinter;
using ESC_POS_USB_NET.Printer;

namespace Celarix.ReceiptPrinter.Extensions
{
    public static class PrinterExtensions
    {
        private const int PrinterMaxColumns = 48;

        public static void PrintWithLineBreaks(this Printer printer, string str)
        {
            var wrappedLines = str.WrapToMaxLength(PrinterMaxColumns);
            foreach (var line in wrappedLines)
            {
                printer.Append(line);
            }
        }

        // From https://download4.epson.biz/sec_pubs/bs/pdf/ESC_POS_FAQ_00.pdf, page 14
        public static void UPCA(this Printer printer, string upcA)
        {
            if (string.IsNullOrWhiteSpace(upcA))
            {
                throw new ArgumentException("UPC-A code cannot be null or empty.", nameof(upcA));
            }

            if (!upcA.All(char.IsDigit) || upcA.Length != 11)
            {
                throw new ArgumentException("UPC-A code must be an 11-digit number.", nameof(upcA));
            }

            var digits = upcA.Select(c => c - '0');
            var oddSumTripled = digits.Where((_, i) => i % 2 == 0).Sum() * 3;
            var evenSum = digits.Where((_, i) => i % 2 != 0).Sum();
            var balancedDigitSum = oddSumTripled + evenSum;
            var checkDigit = balancedDigitSum % 10;
            if (checkDigit != 0)
            {
                checkDigit = 10 - checkDigit;
            }
            
            upcA = upcA + checkDigit.ToString();

            // Barcode command in ESC-POS:
            // GS k m d_1...d_k NUL
            // For UPC-A, m = 0x00, k = 12, d_1...d_k = the UPC-A code
            var commandBytes = new byte[16];
            commandBytes[0] = 0x1D; // GS
            commandBytes[1] = 0x6B; // 'k'
            commandBytes[2] = 0x00; // m, 0x00 for UPC-A
            for (var i = 3; i < 15; i++)
            {
                commandBytes[i] = (byte)upcA[i - 3];
            }
            commandBytes[15] = 0x00; // NUL

            printer.Append(commandBytes);
        }

        public static void SetBarcodeHeight(this Printer printer, byte newHeight)
        {
            // GS h n
            // For setting height, n = newHeight
            var commandBytes = new byte[3];
            commandBytes[0] = 0x1D; // GS
            commandBytes[1] = 0x68; // 'h'
            commandBytes[2] = newHeight;
            printer.Append(commandBytes);
        }

        public static void ResetBarcodeHeight(this Printer printer) => printer.SetBarcodeHeight(162);

        public static void SetHumanReadableBarcodeText(this Printer printer, BarcodeHRIPosition position)
        {
            // GS H n
            // For human-readable text, n = 1
            var commandBytes = new byte[3];
            commandBytes[0] = 0x1D; // GS
            commandBytes[1] = 0x48; // 'H'
            commandBytes[2] = (byte)position;
            printer.Append(commandBytes);
        }
    }
}
