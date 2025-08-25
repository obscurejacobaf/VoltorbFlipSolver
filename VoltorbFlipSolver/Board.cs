using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltorbFlipSolver
{
	public class Board
	{
		public Board()
		{
			Tiles = new Tile[5, 5];
			for (int i = 0; i < 5; i++)
				for (int j = 0; j < 5; j++)
					Tiles[i, j] = new();
		}

		public (int X, int Y) Location = (0, 0);
		public Tile[,] Tiles { get; }
		public Hint[] XHints { get; } = new Hint[5];
		public Hint[] YHints { get; } = new Hint[5];

		public override string ToString()
		{
			var text = "";
			for (int y = 0; y < 5; y++)
			{
				for (int x = 0; x < 5; x++)
				{
					var tile = Tiles[x, y];
					if (tile.Actual == null)
						text += $"[{(tile.PossibeValues.HasFlag(PosibleValues.Bomb) ? '0' : ' ')}{(tile.PossibeValues.HasFlag(PosibleValues.One) ? '1' : ' ')}{(tile.PossibeValues.HasFlag(PosibleValues.Two) ? '2' : ' ')}{(tile.PossibeValues.HasFlag(PosibleValues.Three) ? '3' : ' ')}]";
					else
						text += $"[{tile.Actual}   ]";
				}
				text += "\n";
			}
			return text;
		}
	}

	public class Tile
	{
		public Tile()
		{
			PossibeValues = PosibleValues.Bomb | PosibleValues.One | PosibleValues.Three | PosibleValues.Two;
		}

		public int? Actual { get; set; }
		public decimal? ChanceForBomb { get; set; }
		public PosibleValues PossibeValues { get; set; }
	}

	[Flags]
	public enum PosibleValues
	{
		None = 0,
		Bomb = 1 << 0,
		One = 1 << 1,
		Two = 1 << 2,
		Three = 1 << 3,
	}

	public record struct Hint(int Coins, int Bombs, int Tiles);
}
