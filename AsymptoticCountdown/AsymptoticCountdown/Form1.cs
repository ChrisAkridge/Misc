using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsymptoticCountdown
{
	public partial class Form1 : Form
	{
		private DateTime start;
		private DateTime end;

		private double lastYears;

		public Form1()
		{
			InitializeComponent();
		}

		private void DTPStart_ValueChanged(object sender, EventArgs e)
		{
			start = DTPStart.Value;
		}

		private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
		{
			end = DTPEnd.Value;
		}

		private void TimerMain_Tick(object sender, EventArgs e)
		{
			double progress = GetProgress();
			double asymptoticProgress = GetAsymptoticYears(progress);
			double rate = (asymptoticProgress - lastYears) * 60d;
			lastYears = asymptoticProgress;

			LabelProgress.Text = string.Format("Progress: {0:F8}%", progress * 100d);
			LabelAsymptoticProgress.Text = string.Format("{0} ago ({1:F2} per second)", FormatYear(asymptoticProgress), rate);

			double adjustedProgress = progress * 10000d;

			if (adjustedProgress > 10000d)
			{
				TimerMain.Stop();
				return;
			}

			ProgressOverall.Value = (int)adjustedProgress;
			ProgressHundredth.Value = (int)(AntiTruncate(progress * 100d) * 100d);
			ProgressTenThousandth.Value = (int)(AntiTruncate(progress * 10000d) * 100d);
		}

		private double GetProgress()
		{
			DateTime now = DateTime.Now;
			TimeSpan nowToEnd = end - now;
			TimeSpan startToEnd = end - start;

			double result = (startToEnd - nowToEnd).TotalSeconds / startToEnd.TotalSeconds;
			if (double.IsInfinity(result) || result < 0d) return 0d;
			return result;
		}

		private double GetAsymptoticYears(double percentage)
		{
			double percentageCubed = percentage * percentage * percentage;
			double exponentialTerm = 20.3444d * percentageCubed + 3d;

			double eRaised = Math.Exp(exponentialTerm);
			double eCubed = Math.Exp(3d);

			return eRaised - eCubed;
		}

		private string FormatYear(double years)
		{
			if (years * 365.25d * 24d * 60d < 1d)
			{
				return string.Format("{0:F2} seconds", years * 365.25d * 24d * 60d * 60d);
			}
			else if (years * 365.25d * 24d < 1d)
			{
				return string.Format("{0:F2} minutes", years * 365.25d * 24d * 60d);
			}
			else if (years * 365.25d < 1d)
			{
				return string.Format("{0:F2} hours", years * 365.25d * 24d);
			}
			else if (years < 1d)
			{
				return string.Format("{0:F2} days", years * 365.25d);
			}
			else
			{
				return string.Format("{0:F2} years", years);
			}
		}

		private void TimerRate_Tick(object sender, EventArgs e)
		{
			
		}

		private static double AntiTruncate(double value)
		{
			return (value - Math.Truncate(value));
		}
	}
}
