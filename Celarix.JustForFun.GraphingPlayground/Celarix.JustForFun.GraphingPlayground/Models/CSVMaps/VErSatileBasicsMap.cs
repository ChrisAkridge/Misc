using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Celarix.JustForFun.GraphingPlayground.Models.CSVMaps
{
	internal sealed class VErSatileBasicsMap : ClassMap<VErSatileBasics>
	{
		public VErSatileBasicsMap()
		{
			Map(m => m.DayOfWeek).Index(0);
			Map(m => m.Date).Index(1);
			Map(m => m.DayTitle).Index(2);
			Map(m => m.Score).Index(3);
			Map(m => m.BluebellHours).Index(4);
			Map(m => m.CrimsonHours).Index(5);
			Map(m => m.EmeraldHours).Index(6);
			Map(m => m.SapphireHours).Index(7);
			Map(m => m.StarflowerHours).Index(8);
			Map(m => m.SeashellHours).Index(9);
			Map(m => m.Weight).Index(10);
			Map(m => m.MorningDiastolicBloodPressure).Index(11);
			Map(m => m.MorningSystolicBloodPressure).Index(12);
			Map(m => m.EveningDiastolicBloodPressure).Index(13);
			Map(m => m.EveningSystolicBloodPressure).Index(14);
			Map(m => m.RestingHeartRate).Index(15);
			Map(m => m.MaximumHeartRate).Index(16);
			Map(m => m.HoursInTachycardia).Index(17);
			Map(m => m.TimeToBed).Index(18);
			Map(m => m.SleepHours).Index(19);
			Map(m => m.SleepQuality).Index(20);
			Map(m => m.Exercise).Index(21);
			Map(m => m.HighTemperature).Index(22);
			Map(m => m.Precipitation).Index(23);
			Map(m => m.SaturdayRecord).Index(24);
			Map(m => m.Notes).Index(25);
		}
	}
}
