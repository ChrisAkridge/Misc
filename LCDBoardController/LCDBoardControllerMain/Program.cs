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
			//timer = new Timer(25d);
			bank = new CharacterBank("charbank.bnk");
			//board = new LCDBoard();
			//board.ClearBoard();
			//board.EnableAutoScroll();

			//CustomCharacter football = new CustomCharacter(0x04, 0x0E, 0x1B, 0x1B, 0x1B, 0x1B, 0x0E, 0x04);

			//board.SetBacklightColor(new Color(0, 0, 255));
			var message = new Message("words", bank);
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
