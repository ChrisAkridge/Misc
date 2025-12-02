using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Celarix.JustForFun.LunaGalatea.Logic.Countdown;
using Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds;
using Celarix.JustForFun.LunaGalatea.Providers;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea
{
    public partial class CountdownDetailsForm : Form
    {
        private readonly DateTimeZone easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];
        private readonly IClock clock = SystemClock.Instance;
        private readonly CultureInfo culture = CultureInfo.CurrentCulture;
        private readonly byte[] baseBuffer = new byte[4];

        private IReadOnlyList<Countdown> countdowns;
        private Countdown? selectedCountdown;

        public CountdownDetailsForm()
        {
            InitializeComponent();
        }

        private void CountdownDetailsForm_Load(object sender, EventArgs e)
        {
            countdowns = CountdownProvider.GetStandardCountdowns();
            var now = clock.GetCurrentInstant().InZone(easternTime);

            foreach (var countdown in countdowns)
            {
                ListCountdowns.Items.Add(countdown.Name(now));
            }
            UpdateSelectedCountdown();
        }

        private void ListCountdowns_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIndex = ListCountdowns.SelectedIndex;
            selectedCountdown = countdowns[selectedIndex];
            UpdateSelectedCountdown();
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            UpdateSelectedCountdown();
        }

        private void UpdateSelectedCountdown()
        {
            if (selectedCountdown == null)
            {
                UpdateNullCountdown();
                return;
            }

            var now = clock.GetCurrentInstant().InZone(easternTime);
            var realizedCountdown = CountdownProvider.RealizeCountdown(selectedCountdown, now);

            LabelCountdownName.Text = selectedCountdown.Name(now);
            LabelNextOccurrence.Text = $"Next on {realizedCountdown.NextOccurrence.ToString("yyyy-MM-dd hh:mm:ss tt", culture)}";

            if (realizedCountdown.PreviousOccurence.HasValue)
            {
                LabelProgressLastOccurrence.Text = realizedCountdown.PreviousOccurence.Value.ToString("yyyy-MM-dd", culture);
                ProgressToNextOccurrence.Enabled = true;
                ProgressToNextOccurrence.Maximum = (int)(realizedCountdown.NextOccurrence - realizedCountdown.PreviousOccurence.Value).TotalSeconds;
                var durationSincePrevious = realizedCountdown.DurationSince(now);
                ProgressToNextOccurrence.Value = (int)(durationSincePrevious?.TotalSeconds ?? 0);
            }
            else
            {
                ProgressToNextOccurrence.Enabled = false;
                ProgressToNextOccurrence.Maximum = 100;
                ProgressToNextOccurrence.Value = 0;
                LabelProgressLastOccurrence.Text = "";
            }

            LabelMainCountdown.Text = ToDurationComponents(realizedCountdown.DurationUntil(now));
            LabelProgressNextOccurrence.Text = realizedCountdown.NextOccurrence.ToString("yyyy-MM-dd", culture);
            TextDetails.Text = GetAdvancedDetails(realizedCountdown, now);
        }

        private void UpdateNullCountdown()
        {
            LabelCountdownName.Text = "Please select a countdown!";
            LabelNextOccurrence.Text = string.Empty;
            ProgressToNextOccurrence.Enabled = false;
            ProgressToNextOccurrence.Maximum = 100;
            ProgressToNextOccurrence.Value = 0;
            LabelProgressLastOccurrence.Text = string.Empty;
            LabelProgressNextOccurrence.Text = string.Empty;
            LabelMainCountdown.Text = string.Empty;
            TextDetails.Text = string.Empty;
        }

        private static string ToDurationComponents(Duration duration)
        {
            var builder = new StringBuilder();
            var years = duration.Days / 365;    // yes I know, this is not accurate for leap years, but it's good enough for a countdown
            if (years > 0)
            {
                builder.Append($"{Pluralize(years, "year", "years")}, ");
            }

            var days = duration.Days % 365;
            if (days > 0)
            {
                builder.Append($"{Pluralize(days, "day", "days")}, ");
            }

            var hours = duration.Hours;
            if (hours > 0)
            {
                builder.Append($"{Pluralize(hours, "hour", "hours")}, ");
            }

            var minutes = duration.Minutes;
            if (minutes > 0)
            {
                builder.Append($"{Pluralize(minutes, "minute", "minutes")}, ");
            }

            var seconds = duration.Seconds;
            if (seconds > 0)
            {
                builder.Append(Pluralize(seconds, "second", "seconds"));
            }

            return $"In {builder}";
        }

        private static string Pluralize(int count, string singular, string plural)
        {
            return count == 1 ? $"{count} {singular}" : $"{count} {plural}";
        }

        #region Advanced Details
        // Advanced countdown details for countdowns with or without a previous occurrence:
        // - Total seconds until next occurrence
        // - Total minutes until next occurrence
        // - Total hours until next occurrence
        // - Total days until next occurrence
        // - Total weeks until next occurrence
        // - Log-10 decibel-seconds until next occurrence (10 * log10(total seconds))
        // - Log-2 decibel-seconds until next occurrence (10 * log2(total seconds))
        // If a previous occurrence exists:
        // - XKCD 1017 "Backward in Time" formula
        // - Percentage elapsed since previous occurrence

        private string GetAdvancedDetails(RealizedCountdown countdown, ZonedDateTime now)
        {
            var builder = new StringBuilder();

            Duration durationUntil = countdown.DurationUntil(now);
            var secondsUntil = (int)durationUntil.TotalSeconds;
            var minutesUntil = (int)durationUntil.TotalMinutes;
            var hoursUntil = (int)durationUntil.TotalHours;
            var daysUntil = (int)durationUntil.TotalDays;
            var weeksUntil = daysUntil / 7d;

            var log10decibelSeconds = countdown.DecibelSecondsUntil(now);
            var log2decibelSeconds = 10d * Math.Log10(secondsUntil) / Math.Log10(2d);

            builder.AppendLine($"Total seconds until next occurrence: {NumberWithExtraBases(secondsUntil)}");
            builder.AppendLine($"Total minutes until next occurrence: {NumberWithExtraBases(minutesUntil)}");
            builder.AppendLine($"Total hours until next occurrence: {NumberWithExtraBases(hoursUntil)}");
            builder.AppendLine($"Total days until next occurrence: {NumberWithExtraBases(daysUntil)}");
            builder.AppendLine($"Total weeks until next occurrence: {weeksUntil}");
            builder.AppendLine($"Log-10 decibel-seconds until next occurrence: {log10decibelSeconds:#,###.##}");
            builder.AppendLine($"Log-2 decibel-seconds until next occurrence: {log2decibelSeconds:#,###.##}");

            if (countdown.PreviousOccurence.HasValue)
            {
                var durationSincePrevious = countdown.DurationSince(now);
                if (durationSincePrevious.HasValue)
                {
                    var yearsAgo = XKCD1017Formula(durationSincePrevious.Value.TotalDays / 365.2524d);
                    var xkcd1017Date = ToXKCD1017DateIfInRange(yearsAgo);
                    builder.AppendLine($"XKCD 1017 formula: {yearsAgo:#,###.##} years ago");
                    builder.AppendLine($"XKCD 1017 date: {xkcd1017Date?.ToString("yyyy-MM-dd", culture) ?? "Out of range"}");
                    var totalDuration = countdown.NextOccurrence - countdown.PreviousOccurence.Value;
                    var percentageElapsed = ((totalDuration.TotalSeconds - secondsUntil) / totalDuration.TotalSeconds) * 100d;
                    builder.AppendLine($"Percentage elapsed since previous occurrence: {percentageElapsed:#,###.##}%");
                }
            }

            return builder.ToString().TrimEnd();
        }

        private string NumberWithExtraBases(int number)
        {
            BitConverter.TryWriteBytes(baseBuffer, number);
            var base64String = Convert.ToBase64String(baseBuffer);

            return $"{number:#,###} | 0x{number:X} | {base64String}";
        }

        private double XKCD1017Formula(double progress)
        {
            // From XKCD 1017: https://xkcd.com/1017/
            const double eCubed = 20.085536923187668d; // e^3, where e is Euler's number

            // Build the formula outward:
            var _0 = Math.Pow(progress, 3d);
            var _1 = 20.3444d * _0;
            var _2 = _1 + 3d;
            var _3 = Math.Pow(Math.E, _2);
            var yearsAgo = _3 - eCubed;

            return yearsAgo;
        }

        private ZonedDateTime? ToXKCD1017DateIfInRange(double yearsAgo)
        {
            var now = clock.GetCurrentInstant().InZone(easternTime);
            var year = now.Year - (long)Math.Round(yearsAgo);
            if (year < -9998L)
            {
                // Out of NodaTime's range (https://nodatime.org/2.0.x/userguide/range)
                return null;
            }

            var duration = Duration.FromDays(-yearsAgo * 365.2524d);
            return now.Plus(duration);
        }
        #endregion
    }
}
