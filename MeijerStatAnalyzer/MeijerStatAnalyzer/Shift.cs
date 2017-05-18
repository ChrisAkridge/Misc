using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public sealed class Shift
	{
		private const double AfterTaxAmount = 0.74d;

		public SecondsAfterMidnight StartTime { get; }
		public SecondsAfterMidnight EndTime { get; }
		public double WorkedHours { get; }
		public double WorkedQuarters => WorkedHours * 4d;
		public double WaitHours { get; }
		public double PaidHours { get; }
		public double UnpaidHours { get; }
		public double PayRatePerHour { get; }
		public double Pay => PaidHours * PayRatePerHour;
		public double EstimatedFinalPay => Pay * AfterTaxAmount;

        public ShiftTime ShiftTime
        {
            get
            {
                // 5:00am to 9:00am
                if (StartTime.Seconds >= 18000 && StartTime.Seconds < 32400)
                {
                    return ShiftTime.Open;
                }
                // 9:00am to 4:00pm
                else if (StartTime.Seconds >= 32400 && StartTime.Seconds < 57600)
                {
                    // Before 8:00pm
                    if (EndTime.Seconds < 72000) { return ShiftTime.Midshift; }
                    else { return ShiftTime.Close; }
                }
                // 4:00pm to 8:00pm
                else if (StartTime.Seconds >= 57600 && StartTime.Seconds < 72000)
                {
                    return ShiftTime.Close;
                }
                else { return ShiftTime.ThirdShift; }
            }
        }

		public double TimeAtMeijer => WorkedHours + WaitHours;

		public TimeSpan ShiftLength => (TimeSpan.FromSeconds(EndTime.Seconds) - TimeSpan.FromSeconds(StartTime.Seconds));

		public Shift(SecondsAfterMidnight start, SecondsAfterMidnight end,
		 double hours, double wait, double paidHours, double payRate)
		{
			StartTime = start;
			EndTime = end;
			WorkedHours = hours;
			WaitHours = wait;
			PaidHours = paidHours;
			UnpaidHours = WorkedHours - PaidHours;
			PayRatePerHour = payRate;
		}
	}

    public enum ShiftTime
    {
        Open,
        Midshift,
        Close,
        ThirdShift
    }
}
