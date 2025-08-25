using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltorbFlipSolver
{
	public static class BoardFactory
	{
		public static Board Create(Bitmap bmp)
		{
			var board = new Board();

			// Total pixels on screen 260, 192

			var hintInc = 4;
			for (int i = 0; i < 5; i++)
			{
				board.XHints[i] = CreateHintAt(bmp, 241, hintInc);
				hintInc += 32;
			}

			hintInc = 79;
			for (int i = 0; i < 5; i++)
			{
				board.YHints[i] = CreateHintAt(bmp, hintInc, 164);
				hintInc += 32;
			}

			return board;
		}

		private static Hint CreateHintAt(Bitmap bmp, int gamePixelX, int gamePixelY)
		{
			var coins = NumberMapperHint.GetNumber(bmp, gamePixelX, gamePixelY) * 10;
			coins += NumberMapperHint.GetNumber(bmp, gamePixelX + 8, gamePixelY);
			var bombs = NumberMapperHint.GetNumber(bmp, gamePixelX + 8, gamePixelY + 13);

			return new Hint(coins!.Value, bombs!.Value, 5);
		}
	}
}
