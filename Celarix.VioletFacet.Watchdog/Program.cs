// Check to see if any other watchdogs are running and exit if so.
var existingProcesses = System.Diagnostics.Process.GetProcessesByName("Celarix.VioletFacet.ScheduledHibernator.Watchdog");
if (existingProcesses.Length > 0)
{
    Console.WriteLine("Another instance of the watchdog is already running. Exiting.");
    return;
}

while (true)
{
    // Check for a process named "Celarix.VioletFacet.ScheduledHibernator.exe"
    var processes = System.Diagnostics.Process.GetProcessesByName("Celarix.VioletFacet.ScheduledHibernator");
    if (processes.Length == 0)
    {
        // If the process is not running, start it. It's in the same directory as this executable, so we can just start it by name.
        try
        {
            System.Diagnostics.Process.Start("Celarix.VioletFacet.ScheduledHibernator.exe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start Celarix.VioletFacet.ScheduledHibernator.exe: {ex.Message}");
        }
    }

    // Sleep for 1 second before checking again
    await Task.Delay(1000);
}