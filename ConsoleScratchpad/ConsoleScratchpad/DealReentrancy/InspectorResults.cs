using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScratchpad.DealReentrancy
{
    internal enum BuyerStepState
    {
        NoBuyers,
        BuyerOnly,
        BuyerHighCreditScoreWithCoBuyer,
        BuyerWithCoBuyerHighCreditScore,
        ToBeRun,
        StepComplete
    }

    internal enum TradeInStepState
    {
        NoDispositionSpecified,
        WillNotHaveTrade,
        WillHaveTrade,
        HasTrade,
        ToBeRun,
        StepComplete
    }

    internal sealed class InspectorResults
    {
        public const string Header = "Buyer,Trade-In,Credit/Customize Payment,Credit Application Flow";

        public BuyerStepState Buyer { get; set; }
        public TradeInStepState TradeIn { get; set; }
        public bool CreditAndPayment { get; set; }

        public bool CreditApplicationFlow { get; set; }

        public override string ToString()
        {
            string[] validFlags =
            {
                Buyer.ToString(),
                TradeIn.ToString(),
                (CreditAndPayment) ? "Yes" : "No",
                (CreditApplicationFlow) ? "Yes" : "No"
            };

            return string.Join(",", validFlags);
        }

        public override bool Equals(object obj)
        {
            var that = (InspectorResults)obj;
            return Buyer == that.Buyer
                && TradeIn == that.TradeIn
                && CreditAndPayment == that.CreditAndPayment
                && CreditApplicationFlow == that.CreditApplicationFlow;
        }

        public bool IsLegal()
        {
            bool buyerIsLegal = true;
            bool tradeInIsLegal = true;
            bool creditAndPayment = (Buyer != BuyerStepState.NoBuyers) || !CreditAndPayment;
            bool creditApplicationFlowIsLegal = CreditAndPayment || !CreditApplicationFlow;

            return buyerIsLegal && tradeInIsLegal && creditAndPayment && creditApplicationFlowIsLegal;
        }

        public InspectorResults Clone()
        {
            var output = new InspectorResults();
            output.Buyer = Buyer;
            output.TradeIn = TradeIn;
            output.CreditAndPayment = CreditAndPayment;
            output.CreditApplicationFlow = CreditApplicationFlow;
            return output;
        }

        public InspectorResults ShouldRunSteps()
        {
            var output = new InspectorResults();
            output.Buyer = (Buyer == BuyerStepState.NoBuyers) ? BuyerStepState.ToBeRun : BuyerStepState.StepComplete;
            output.TradeIn = (TradeIn == TradeInStepState.NoDispositionSpecified) ? TradeInStepState.ToBeRun : TradeInStepState.StepComplete;
            output.CreditAndPayment = !CreditAndPayment;
            output.CreditApplicationFlow = !CreditApplicationFlow;
            return output;
        }

        public static InspectorResults FromNumber(int number)
        {
            if (number > 63)
            {
                throw new ArgumentOutOfRangeException();
            }

            var output = new InspectorResults();

            output.CreditApplicationFlow = (number % 2 == 0);
            number /= 2;

            output.CreditAndPayment = (number % 2 == 0);
            number /= 2;

            switch (number % 4)
            {
                case 0:
                    output.TradeIn = TradeInStepState.NoDispositionSpecified;
                    break;
                case 1:
                    output.TradeIn = TradeInStepState.WillNotHaveTrade;
                    break;
                case 2:
                    output.TradeIn = TradeInStepState.WillHaveTrade;
                    break;
                case 3:
                    output.TradeIn = TradeInStepState.HasTrade;
                    break;
            }

            number /= 4;

            switch (number % 4)
            {
                case 0:
                    output.Buyer = BuyerStepState.NoBuyers;
                    break;
                case 1:
                    output.Buyer = BuyerStepState.BuyerOnly;
                    break;
                case 2:
                    output.Buyer = BuyerStepState.BuyerHighCreditScoreWithCoBuyer;
                    break;
                case 3:
                    output.Buyer = BuyerStepState.BuyerWithCoBuyerHighCreditScore;
                    break;
            }

            return output;
        }
    }
}
