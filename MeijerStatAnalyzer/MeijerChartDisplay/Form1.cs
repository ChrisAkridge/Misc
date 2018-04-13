using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using MeijerStatAnalyzer;
using LiveCharts.Defaults;

namespace MeijerChartDisplay
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			StatsByDay stats = Parser.Parse();
			IEnumerable<Shift> shifts = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift);
			IEnumerable<string> displayedDates = shifts.Select(s => s.Date.ToShortDateString());

			NumberOfWorkingDays(stats);
		}

		private void LineMap(IEnumerable<Shift> shifts, IEnumerable<string> displayedDates)
		{
			IEnumerable<double> workedHours = shifts.Select(s => s.WorkedHours);
			IEnumerable<double> workedRollingAverage = RollingAverage(workedHours);
			IEnumerable<double> workedRollingSum = RollingSum(workedHours);

			IEnumerable<double> waitHours = shifts.Select(s => s.WaitHours);
			IEnumerable<double> waitRollingAverage = RollingAverage(waitHours);
			IEnumerable<double> waitRollingSum = RollingSum(waitHours);

			cartesianChart1.AxisX.Add(new Axis
			{
				Title = "Date",
				Labels = displayedDates.ToList(),
			});

			cartesianChart1.AxisY.Add(new Axis 
			{
				MinValue = 0,
				LabelFormatter = value => value.ToString()
			});

			cartesianChart1.Series = new SeriesCollection
			{
				new LineSeries
				{
					Title = "Wait Hours",
					Values = new ChartValues<double>(waitRollingSum)
				}
			};
		}

		private void ShiftCountByDayOfMonth(IEnumerable<Shift> shifts, IEnumerable<string> displayedDates)
		{
			var shiftsByDayOfMonth = shifts.GroupBy(s => s.Date.Day).OrderBy(g => g.Key).ToList();

			cartesianChart1.AxisX.Add(new Axis
			{
				Title = "Day of Month",
				Labels = Enumerable.Range(1, 31).Select(i => i.ToString()).ToList()
			});

			cartesianChart1.Series = new SeriesCollection
			{
				new ColumnSeries
				{
					Title = "Shifts on this Day of Month",
					Values = new ChartValues<int>(shiftsByDayOfMonth.Select(s => s.Count()))
				}
			};
		}

		private void ShiftCountByWeekday(IEnumerable<Shift> shifts)
		{
			var shiftsByDayOfWeek = shifts.GroupBy(s => s.Date.DayOfWeek).OrderBy(g => g.Key).ToList();

			cartesianChart1.AxisX.Add(new Axis
			{
				Title = "Weekday",
				Labels = Enumerable.Range(1, 7).Select(i => ((DayOfWeek)(i-1)).ToString()).ToList()
			});

			cartesianChart1.Series = new SeriesCollection
			{
				new ColumnSeries
				{
					Title = "Shifts on this Weekday",
					Values = new ChartValues<int>(shiftsByDayOfWeek.Select(s => s.Count()))
				}
			};
		}

		private void NumberOfWorkingDays(StatsByDay stats)
		{
			var values = new List<int>();
			int workingDayCount = 0;

			foreach (MeijerStatAnalyzer.Day day in stats.Days)
			{
				if (day.Type == DayType.Working)
				{
					workingDayCount++;
				}
				values.Add(workingDayCount);
			}

			cartesianChart1.Series = new SeriesCollection
			{
				new LineSeries
				{
					Title = "Number of Working Days on this Day Number",
					Values = new ChartValues<int>(values)
				}
			};
		}

		private IEnumerable<double> RollingAverage(IEnumerable<double> sequence)
		{
			double sum = 0d;
			int count = 0;
			foreach (double d in sequence)
			{
				sum += d;
				count++;
				yield return (sum / count);
			}
		}

		private IEnumerable<double> RollingSum(IEnumerable<double> sequence)
		{
			double sum = 0d;
			foreach (double d in sequence)
			{
				sum += d;
				yield return sum;
			}
		}
	}
}
