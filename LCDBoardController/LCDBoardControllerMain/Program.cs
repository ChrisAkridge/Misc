using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CharacterManager;
using LCDBoardController;

namespace LCDBoardControllerMain
{
	public class Program
	{
		private static Timer timer;
		private static LCDBoard board;
		private static CharacterBank bank;

		static void Main(string[] args)
		{
			timer = new Timer(25d);
			bank = new CharacterBank("charbank.bnk");
			board = new LCDBoard();
			board.ClearBoard();

			CustomCharacter football = new CustomCharacter(0x04, 0x0E, 0x1B, 0x1B, 0x1B, 0x1B, 0x0E, 0x04);
			// bank.Add("football", football);
			// GenerateRandomCharacters();
			// bank.WriteToFile("charbank.bnk");

			Random random = new Random();
			for (int i = 0; i < 8; i++)
			{
				int index = random.Next(0, 999);
				var loadBytes = bank[index.ToString()].GenerateLoadBytes(i);
				board.Send(loadBytes);
				board.Send(new byte[] {(byte)i});
				board.SetCursorPosition(i + 1, 1);
			}
			
			Console.ReadKey();
		}

		static void GenerateRandomCharacters()
		{
			Random random = new Random();

			for (int i = 0; i < 1000; i++)
			{
				string name = i.ToString();
				byte[] pattern = new byte[8];
				random.NextBytes(pattern);
				bank.Add(name, new CustomCharacter(pattern));
			}
		}

		static void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Random random = new Random();
			
			board.SetCursorPosition(random.Next(1, 17), random.Next(1, 3));
			board.Write("X");
		}
	}
}
