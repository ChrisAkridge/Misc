using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter
{
    public enum BarcodeHRIPosition : byte
    {
        Disabled = 0,
        Above = 1,
        Below = 2,
    }

    public enum NumberedAnniversaryKind
    {
        Birthday,
        Anniversary,
        RomanNumeral,
    }
}
