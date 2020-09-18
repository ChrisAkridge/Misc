using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScratchpad.DealReentrancy
{
    public static class TableMaker
    {
        private static readonly string[] DealActions =
        {
            "VDP Lease/Loan is Clicked",
            "VDP Cash Button is Clicked",
            "Change Buyer",
            "Change Co-Buyer",
            "Change existing Trade In",
            "Delete existing Trade In",
            "Add additional Trade In",
            "Delete Buyer",
            "Delete Co-Buyer",
            "Swap Buyer and Co-Buyer",
            "Change existing loan/lease",
            "Change to cash",
            "Change anything on credit app"
        };

        private static InspectorResults InvalidateOnDealAction(InspectorResults input, string action)
        {
            var output = input.Clone();

            switch (action)
            {
                case "VDP Lease/Loan is Clicked":
                case "VDP Cash Button is Clicked":
                case "Change Buyer":
                case "Change Co-Buyer":
                case "Change existing Trade In":
                case "Delete existing Trade In":
                case "Add additional Trade In":
                    output.CreditAndPayment = output.CreditApplicationFlow = false;
                    break;
                case "Delete Buyer":
                    if (output.Buyer == BuyerStepState.BuyerOnly)
                    {
                        output.Buyer = BuyerStepState.NoBuyers;
                        output.CreditAndPayment = false;
                    }
                    else if (output.Buyer == BuyerStepState.BuyerWithCoBuyerHighCreditScore)
                    {
                        output.Buyer = BuyerStepState.BuyerOnly;
                    }
                    else if (output.Buyer == BuyerStepState.BuyerHighCreditScoreWithCoBuyer)
                    {
                        output.Buyer = BuyerStepState.BuyerOnly;
                        output.CreditAndPayment = false;
                    }
                    output.CreditApplicationFlow = false;
                    break;
                case "Delete Co-Buyer":
                    if (output.Buyer == BuyerStepState.BuyerWithCoBuyerHighCreditScore)
                    {
                        output.CreditAndPayment = false;
                    }
                    output.Buyer = BuyerStepState.BuyerOnly;
                    output.CreditApplicationFlow = false;
                    break;
                case "Swap Buyer and Co-Buyer":
                case "Change existing loan/lease":
                case "Change to cash":
                case "Change anything on current applicant or co-applicant":
                    output.CreditApplicationFlow = false;
                    break;
                default:
                    break;
            }

            return output;
        }

        public static string[] MakeTable()
        {
            string inspectorHeader = InspectorResults.Header;
            string header = string.Join(",", "Deal Action", inspectorHeader, inspectorHeader);
            var file = new List<string>
            {
                header
            };

            foreach (var action in DealActions)
            {
                for (int i = 0; i < 64; i++)
                {
                    var initialInspectorValues = InspectorResults.FromNumber(i);
                    if (!initialInspectorValues.IsLegal()) { continue; }
                    var transformedInspectorValues = InvalidateOnDealAction(initialInspectorValues, action);
                    if (initialInspectorValues.Equals(transformedInspectorValues)) { continue; }
                    file.Add(string.Join(",", action, initialInspectorValues.ToString(), transformedInspectorValues.ShouldRunSteps().ToString()));
                }
            }

            return file.ToArray();
        }
    }
}
