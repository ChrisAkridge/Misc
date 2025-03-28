using Celarix.JustForFun.GraphingPlayground.Logic;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			//var generator =
			//	new MeijerDeepStatisticsGenerator(
			//		$"C:\\Users\\cakri\\OneDrive\\Documents\\Just for Fun\\Meijer Deep Statistics");
			//generator.Generate();

			//return;
			
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.Run(new PlaygroundListForm());
		}
	}
}