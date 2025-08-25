using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltorbFlipSolver
{
	public static class NumberMapperHint
	{
		private record NumberPattern(int Value, bool[,] Pixels);
		private static IEnumerable<NumberPattern> PossibleNumbers { get; } =
		[
			new(0, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, false, false, true, true },
				{ true, true, false, false, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
			new(1, new bool[,]
			{
				{ false, false, true, true, false, false },
				{ false, true, true, true, false, false },
				{ false, true, true, true, false, false },
				{ false, false, true, true, false, false },
				{ false, false, true, true, false, false },
				{ false, false, true, true, false, false },
				{ false, true, true, true, true, false },
				{ false, true, true, true, true, false },
			}),
			new(2, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ false, false, false, true, true, true },
				{ false, false, true, true, true, false },
				{ false, true, true, true, false, false },
				{ true, true, true, true, true, true },
				{ true, true, true, true, true, true },
			}),
			new(3, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ false, false, true, true, true, true },
				{ false, false, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
			new(4, new bool[,]
			{
				{ true, true, false, false, false, false },
				{ true, true, false, true, true, false },
				{ true, true, false, true, true, false },
				{ true, true, false, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, true, true, true, true },
				{ false, false, false, true, true, false },
				{ false, false, false, true, true, false },
			}),
			new(5, new bool[,]
			{
				{ true, true, true, true, true, true },
				{ true, true, true, true, true, true },
				{ true, true, false, false, false, false },
				{ true, true, true, true, true, false },
				{ false, false, false, false, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
			new(6, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, false, false },
				{ true, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
			new(7, new bool[,]
			{
				{ true, true, true, true, true, true },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ false, false, false, true, true, true },
				{ false, false, false, true, true, false },
				{ false, false, true, true, true, false },
				{ false, false, true, true, false, false },
				{ false, false, true, true, false, false },
			}),
			new(8, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
			new(9, new bool[,]
			{
				{ false, true, true, true, true, false },
				{ true, true, true, true, true, true },
				{ true, true, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, true },
				{ false, false, false, false, true, true },
				{ true, true, true, true, true, true },
				{ false, true, true, true, true, false },
			}),
		];

		public static int? GetNumber(Bitmap bmp, int gamePixelX, int gamePixelY, bool allowNull = false)
		{
			var number = new bool[6,8];
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 6; x++)
				{
					var pixelX = (int)Math.Ceiling((decimal)(gamePixelX + x) / (decimal)260 * (decimal)bmp.Width);
					var pixelY = (int)Math.Ceiling((decimal)(gamePixelY + y) / (decimal)192 * (decimal)bmp.Height);
					if (gamePixelX == 143)
						pixelX += 2;
					if (gamePixelX == 151)
						pixelX += 4;
					if (gamePixelX == 175)
						pixelX += 6;
					if (gamePixelX == 183)
						pixelX += 8;
					if (gamePixelX == 207)
						pixelX += 10;
					if (gamePixelX == 215)
						pixelX += 10;
					var color = bmp.GetPixel(pixelX, pixelY);

					number[x, y] = color.R + color.G + color.B < 200;
				}
			}

			foreach (var expectedNum in PossibleNumbers)
			{
				bool areEqual = true;
				for (int x = 0; x < 6; x++)
				{
					for (int y = 0; y < 8; y++)
					{
						if (number[x, y] != expectedNum.Pixels[y, x])
						{
							areEqual = false;
							break;
						}
					}
					if (!areEqual)
						break;
				}
				if (areEqual)
					return expectedNum.Value;
			}

			var initX = (int)Math.Ceiling((decimal)(gamePixelX) / (decimal)260 * (decimal)bmp.Width);
			var initY = (int)Math.Ceiling((decimal)(gamePixelY) / (decimal)192 * (decimal)bmp.Height);

			if (allowNull)
			{
				var color = bmp.GetPixel(initX, initY);
				if ((color.R == 24 && color.G == 132 && color.B == 99) ||
					(color.R == 41 && color.G == 165 && color.B == 107))
					return null;
				return -1;
			}
			throw new Exception($"Number at {initX} {initY} did not match anything.");
		}
	}
}
