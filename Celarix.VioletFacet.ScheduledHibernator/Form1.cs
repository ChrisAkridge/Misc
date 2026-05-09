using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Celarix.VioletFacet.ScheduledHibernator
{
    public partial class Form1 : Form
    {
        // Winlogon.exe icon redesign for Windows 11
        // From https://www.reddit.com/r/Windows_Redesign/comments/1madxbc/winlogonexe_icons_in_xp_vista7881_10_and_11_styles/
        // by /u/supsmashpastel

        private enum WatchdogStatus
        {
            Running,
            Launching,
            NotFound
        }

        private enum CountdownState
        {
            None,
            AwaitingOverride,
            AwaitingRetry,
            OverrideActive
        }

        private Config config;
        private Queue<ScheduledEvent> scheduledEvents = new Queue<ScheduledEvent>();
        private Random random = new Random();
        private bool updateScheduled = false;
        private WatchdogStatus watchdogStatus;
        private bool nextHibernateSkipped = false;
        private int keypressCount = 0;
        private CountdownState countdownState = CountdownState.None;
        private int secondsLeftInCountdown = 0;

        public Form1()
        {
            InitializeComponent();

            CheckAndInitialize();
        }

        private void CheckAndInitialize()
        {
            // Look for a folder in %appdata%\VioletFacet\ScheduledHibernator
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFolderPath = Path.Combine(appDataPath, "VioletFacet", "Scheduled Hibernator");
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            // Check that config.json and config.bak exist, and if not, create them with default values
            var configFilePath = Path.Combine(configFolderPath, "config.json");
            var backupConfigFilePath = Path.Combine(configFolderPath, "config.bak");

            // The watchdog should be in the same directory as this executable, so we can set the default path to be there.
            var executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var watchdogDefaultPath = Path.Combine(executableDirectory, "Celarix.VioletFacet.Watchdog.exe");

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    Sunday = new TimeOnly(0, 0),
                    Monday = new TimeOnly(0, 0),
                    Tuesday = new TimeOnly(0, 0),
                    Wednesday = new TimeOnly(0, 0),
                    Thursday = new TimeOnly(0, 0),
                    Friday = new TimeOnly(0, 0),
                    Saturday = new TimeOnly(0, 0),
                    WatchdogExecutablePath = watchdogDefaultPath
                };
                config = defaultConfig;
                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, json);
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                File.WriteAllText(backupConfigFilePath, base64);

                // Set the backup as a hidden file as a minor deterrent against me.
                File.SetAttributes(backupConfigFilePath, FileAttributes.Hidden);
            }
            else
            {
                // Don't trust the config file, read the backup config file and use it.
                // This is to prevent me from closing this app, editing the config file,
                // and having the scheduled hibernation to a different time than what I intend now.
                var backup = File.ReadAllText(backupConfigFilePath);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(backup));
                config = JsonSerializer.Deserialize<Config>(json);

                // Also overwrite the config file. This is a little harsh, since maybe I performed a valid
                // override and modified the file, waiting for the 12:00pm next day update, but present-me
                // wants to prevent shenanigans more than assume the best of future-me.
                File.WriteAllText(configFilePath, json);
            }

            UpdateScheduleListView();
            RebuildScheduledEvents();
        }

        private void RebuildScheduledEvents()
        {
            scheduledEvents.Clear();
            var now = DateTimeOffset.Now;
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                var hibernationTimeOnly = config.GetHibernationTimeForDay(day);
                var hibernationTime = new DateTimeOffset(now.Year, now.Month, now.Day, hibernationTimeOnly.Hour, hibernationTimeOnly.Minute, 0, now.Offset)
                    .AddDays((7 + day - now.DayOfWeek) % 7); // Get the next occurrence of the specified day
                scheduledEvents.Enqueue(new ScheduledEvent { EventTime = hibernationTime, EventType = EventType.Hibernate });
            }
        }

        private void HibernateComputer()
        {
            // Dequeue the current hibernation event since we're executing it now.
            if (scheduledEvents.Count > 0 && scheduledEvents.Peek().EventType == EventType.Hibernate)
            {
                var hibernationEvent = scheduledEvents.Dequeue();
                // Enqueue the next hibernation event for the same day of the week, but for the next week, to maintain the schedule.
                var nextHibernationTime = hibernationEvent.EventTime.AddDays(7);
                scheduledEvents.Enqueue(new ScheduledEvent { EventTime = nextHibernationTime, EventType = EventType.Hibernate });
            }

            // Invoke shutdown.exe with the /h flag to hibernate the computer.
            var process = new Process();
            process.StartInfo.FileName = "shutdown.exe";
            process.StartInfo.Arguments = "/h";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            // No need to wait for the process to exit, as the computer will hibernate immediately.
        }

        private void UpdateSchedule()
        {
            if (!updateScheduled) { return; }

            if (scheduledEvents.Count > 0 && scheduledEvents.Peek().EventType == EventType.SaveSettings)
            {
                scheduledEvents.Dequeue();
            }

            // At this point, I've modified config.json, so we can now save it to the backup.
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFolderPath = Path.Combine(appDataPath, "VioletFacet", "Scheduled Hibernator");
            var configFilePath = Path.Combine(configFolderPath, "config.json");
            var backupConfigFilePath = Path.Combine(configFolderPath, "config.bak");
            var json = File.ReadAllText(configFilePath);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            File.WriteAllText(backupConfigFilePath, base64);

            // We can also update the in-memory config object now.
            config = JsonSerializer.Deserialize<Config>(json);
            RebuildScheduledEvents();
            UpdateScheduleListView();

            updateScheduled = false;
        }

        private void UpdateScheduleListView()
        {
            var scheduledListView = listView1;
            scheduledListView.Items.Clear();
            var daysOfWeek = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var times = new[] { config.Sunday, config.Monday, config.Tuesday, config.Wednesday, config.Thursday, config.Friday, config.Saturday };
            var timeDisplays = times.Select(t => t.ToString("hh:mm tt")).ToArray();
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                var item = new ListViewItem(daysOfWeek[i]);
                item.SubItems.Add(timeDisplays[i]);
                scheduledListView.Items.Add(item);
            }
        }

        private WatchdogStatus GetWatchdogStatus()
        {
            var executablePath = config.WatchdogExecutablePath;

            if (string.IsNullOrWhiteSpace(executablePath))
            {
                watchdogStatus = WatchdogStatus.NotFound;
                return watchdogStatus;
            }

            var executableName = Path.GetFileName(executablePath);
            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executableName));
            if (runningProcesses.Length > 0)
            {
                watchdogStatus = WatchdogStatus.Running;
                // Dispose the array to free handles
                foreach (var process in runningProcesses)
                {
                    process?.Dispose();
                }
            }
            else
            {
                watchdogStatus = WatchdogStatus.NotFound;
            }

            label7.Text = $"Watchdog Status: {watchdogStatus}";
            return watchdogStatus;
        }

        private void LaunchWatchdog()
        {
            var executablePath = config.WatchdogExecutablePath;

            if (!File.Exists(executablePath))
            {
                // Future-me, I know you want to stay up. Maybe you're fighting with me because I wrote
                // a bug here or something, but... honestly... it sucks being up until 4:30am. Really
                // and seriously sucks. This watchdog is coming back.
                RestoreWatchdog();
            }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = executablePath;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                watchdogStatus = WatchdogStatus.Launching;
            }
            catch (Exception ex)
            {
                // Handle launch failure
                watchdogStatus = WatchdogStatus.NotFound;
            }
        }

        private void RestoreWatchdog()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Celarix.VioletFacet.ScheduledHibernator.Resources.Celarix.VioletFacet.Watchdog.exe";

                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                    }

                    var executablePath = config.WatchdogExecutablePath;

                    if (string.IsNullOrWhiteSpace(executablePath))
                    {
                        throw new InvalidOperationException("Watchdog executable path is not configured.");
                    }

                    var directory = Path.GetDirectoryName(executablePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var fileStream = new FileStream(executablePath, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle appropriately
                MessageBox.Show($"Failed to restore watchdog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openOverrideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                var now = DateTimeOffset.Now;

                watchdogStatus = GetWatchdogStatus();
                if (watchdogStatus == WatchdogStatus.NotFound)
                {
                    // Oh no you don't.
                    LaunchWatchdog();
                }

                if (countdownState == CountdownState.AwaitingOverride)
                {
                    secondsLeftInCountdown -= 1;
                    button2.Text = $"Override in {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}";
                    if (secondsLeftInCountdown == 0)
                    {
                        // Override successful!
                        ActivateOverride();
                    }
                }
                else if (countdownState == CountdownState.AwaitingRetry)
                {
                    secondsLeftInCountdown -= 1;
                    button2.Text = $"Retry in {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}";
                    if (secondsLeftInCountdown == 0)
                    {
                        button2.Text = "Try Override";
                        button2.Enabled = true;
                        label4.Text = Resources.OverrideTexts.GetRandomText();
                        textBox1.Text = string.Empty;
                        keypressCount = 0;
                        label5.Text = $"Keypresses: {keypressCount}";
                        countdownState = CountdownState.None;
                    }
                }
                else if (countdownState == CountdownState.OverrideActive)
                {
                    secondsLeftInCountdown -= 1;
                    label6.Text = $"System is disarmed for {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}!";
                    if (secondsLeftInCountdown == 0)
                    {
                        DeactivateOverride();
                    }
                }

                var nextEvent = scheduledEvents.Count > 0 ? scheduledEvents.Peek() : null;
                if (nextEvent == null)
                {
                    label8.Text = "WARNING! No Next Event!";
                    return;
                }

                if (!nextEvent.EventPassed(DateTimeOffset.Now))
                {
                    var timeUntilNextEvent = nextEvent.EventTime - now;
                    label8.Text = $"Next Event: {nextEvent.EventType}\r\nat {nextEvent.EventTime:hh:mm tt}\r\n(in {timeUntilNextEvent:hh:mm:ss})";
                    return;
                }

                var eventType = nextEvent.EventType;
                if (eventType == EventType.Hibernate)
                {
                    if (nextHibernateSkipped)
                    {
                        nextHibernateSkipped = false;
                        button3.Text = "Skip Tonight";
                        button3.Enabled = true;
                        scheduledEvents.Dequeue();
                    }
                    else
                    {
                        HibernateComputer();
                    }
                }
                else if (eventType == EventType.SaveSettings)
                {
                    UpdateSchedule();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle appropriately
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivateOverride()
        {
            countdownState = CountdownState.OverrideActive;
            secondsLeftInCountdown = 10 * 60; // 10 minutes
            button2.Text = "Override Activated";
            label6.Text = $"System is disarmed for {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}!";
            label6.ForeColor = Color.Green;
            button3.Enabled = true;
            button1.Enabled = true;
            textBox1.Clear();
            keypressCount = 0;
            label5.Text = $"Keypresses: {keypressCount}";
            label4.Text = Resources.OverrideTexts.GetRandomText();
        }

        private void DeactivateOverride()
        {
            countdownState = CountdownState.None;
            secondsLeftInCountdown = 0;
            button2.Text = "Try Override";
            label6.Text = $"System is armed.";
            label6.ForeColor = Color.Red;
            button3.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Just open the config file in the system default text editor.
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFolderPath = Path.Combine(appDataPath, "VioletFacet", "Scheduled Hibernator");
            var configFilePath = Path.Combine(configFolderPath, "config.json");
            Process.Start(new ProcessStartInfo
            {
                FileName = configFilePath,
                UseShellExecute = true
            });

            updateScheduled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (nextHibernateSkipped)
            {
                nextHibernateSkipped = false;
                button3.Text = "Skip Tonight";
            }
            else
            {
                nextHibernateSkipped = true;
                button3.Text = "Unskip Tonight";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label4.Text = Resources.OverrideTexts.GetRandomText();

            // Hide the form on load, since the user doesn't need to interact with it until they want to override.
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsValidKeypress(e.KeyCode))
            {
                keypressCount++;
                label5.Text = $"Keypresses: {keypressCount}";
            }

            var input = textBox1.Text;
            var target = label4.Text;
            if (target.StartsWith(input, StringComparison.Ordinal))
            {
                // All good, keep going.
            }
            else
            {
                var charsToRemove = Math.Min(random.Next(1, 6), textBox1.Text.Length);
                if (charsToRemove > 0)
                {
                    textBox1.Text = textBox1.Text[..^charsToRemove];
                    textBox1.SelectionStart = textBox1.Text.Length; // Keep cursor at end
                }
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private bool IsValidKeypress(Keys keyCode)
        {
            return (keyCode >= Keys.A && keyCode <= Keys.Z) ||
                   (keyCode >= Keys.D0 && keyCode <= Keys.D9) ||
                   (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9) ||
                   keyCode == Keys.Space ||
                   keyCode == Keys.OemPeriod ||
                   keyCode == Keys.Oemcomma ||
                   // Add other symbol keys as needed
                   (keyCode >= Keys.Oem1 && keyCode <= Keys.OemBackslash);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var input = textBox1.Text;
            var target = label4.Text;
            var inputMatches = target.Equals(input, StringComparison.Ordinal);
            var keypressesSufficient = keypressCount >= target.Length;
            var overrideSuccessful = inputMatches && keypressesSufficient;

            if (overrideSuccessful)
            {
                countdownState = CountdownState.AwaitingOverride;
                secondsLeftInCountdown = 2 * 60; // 2 minutes
                button2.Enabled = false;
                button2.Text = $"Override in {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}";
            }
            else
            {
                // Add retry logic for failed attempts
                countdownState = CountdownState.AwaitingRetry;
                secondsLeftInCountdown = 30; // 30 second penalty
                button2.Enabled = false;
                button2.Text = $"Retry in {SecondsToMinutesAndSeconds(secondsLeftInCountdown)}";
            }
        }

        private string SecondsToMinutesAndSeconds(int totalSeconds)
        {
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:D2}";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Trying to close the form? Follow this one priciple: D.O.N.T.
            // Don't. Ever
            e.Cancel = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Check for minizing and hide the form if so, since the user doesn't need to interact with it until they want to override.
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                Hide();
            }
        }

        private void SendHibernationAppNotification(int minutesRemaining)
        {
            new ToastContentBuilder()
                .AddText("Scheduled Hibernation")
                .AddText($"Your computer will hibernate in {minutesRemaining} minutes.")
                .Show();
        }
    }
}
