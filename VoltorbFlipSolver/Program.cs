using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VoltorbFlipSolver
{
#pragma warning disable CA1416 // Validate platform compatibility
	internal class Program
	{
		static void Main()
		{
			string windowTitle = "infinitefusion";
			IntPtr window = DwmWindowCapture.GetWindowByTitle(windowTitle);

			// Total pixels on screen 260, 192
			//1920, 1440
			//{
			//screenshot.Save("aTest.png", ImageFormat.Png);
			//ImageCropper.SaveRegion(screenshot, new(1118, 1230, 100, 100), "hint1.png");
			//}
			while (true)
			{
				try
				{
					PlayGame(window);
				}
				catch (Exception ex)
				{
					var qwe = 456;
				}
				var asd = 123;
			}
		}

		private static void PlayGame(IntPtr window)
		{
			var screenshot = DwmWindowCapture.CaptureWindow(window);
			var board = BoardFactory.Create(screenshot);
			while (true)
			{
				Task.Delay(50).Wait();
				screenshot = DwmWindowCapture.CaptureWindow(window);

				if (HasWon(screenshot, window))
					return;

				ScanBoard(board, screenshot);
				Console.WriteLine(board.ToString());

				if (HasLost(board, window))
					return;

				if (Stage1(board, window))
					continue;

				if (Stage2(board))
					continue;

				if (Stage3(board))
					continue;

				if (Stage4(board))
					continue;

				if (Stage5(board))
					continue;

				if (Stage6(board, window))
					continue;

				Stage7(board);
				if (MarkAllNoVoltorbs(board, window))
					continue;

				CalculateRisk(board);
				if (MarkLowestRisk(board, window))
				{
					Task.Delay(50).Wait();
					continue;
				}

				return;
			}
		}

		private static bool MarkLowestRisk(Board board, nint window)
		{
			Tile? lowestRiskTile = null;
			int lrx = 0;
			int lry = 0;

			for (int x = 0; x < 5; x++)
			{
				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					// Always use the lowest risk, if a draw, use the tile that is least likely to be 1.
					if (lowestRiskTile == null ||
						tile.ChanceForBomb < lowestRiskTile.ChanceForBomb)
					{
						lowestRiskTile = tile;
						lrx = x;
						lry = y;
					}
					else if (tile.ChanceForBomb == lowestRiskTile.ChanceForBomb &&
						lowestRiskTile.PossibeValues.HasFlag(PosibleValues.One))
					{
						if (!lowestRiskTile.PossibeValues.HasFlag(PosibleValues.Two) && !lowestRiskTile.PossibeValues.HasFlag(PosibleValues.Three))
						{
							lowestRiskTile = tile;
							lrx = x;
							lry = y;
						}
						else if (!tile.PossibeValues.HasFlag(PosibleValues.One))
						{
							lowestRiskTile = tile;
							lrx = x;
							lry = y;
						}
					}
				}
			}

			if (lowestRiskTile != null)
			{
				SetTile(board.Location, lrx, lry, window);
				board.Location = (lrx, lry);
				return true;
			}
			return false;
		}

		private static void CalculateRisk(Board board)
		{
			for (int x = 0; x < 5;x++)
			{
				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					var hintX = ActualHintX(board, y);
					var hintY = ActualHintY(board, x);

					tile.ChanceForBomb = 1 - ((1 - ((decimal)hintX.Bombs / (decimal)hintX.Tiles)) * (1 - ((decimal)hintY.Bombs / (decimal)hintY.Tiles)));
				}
			}
		}

		private static bool HasLost(Board board, IntPtr window)
		{
			for (int x = 0; x < 5; x++)
			{
				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null && tile.Actual == -1)
					{
						Task.Delay(1000).Wait();
						KeyboardInput.SendEnter(window);
						Task.Delay(1000).Wait();
						KeyboardInput.SendEnter(window);
						Task.Delay(2000).Wait();
						KeyboardInput.SendEnter(window);
						Task.Delay(1000).Wait();
						return true;
					}
				}
			}
			return false;
		}

		private static bool HasWon(Bitmap board, IntPtr window)
		{
			var pixel = board.GetPixel(0, 0);
			if (pixel.R == 25 && pixel.G == 100 && pixel.B == 65)
			{
				Task.Delay(1000).Wait();
				KeyboardInput.SendEnter(window);
				Task.Delay(50).Wait();
				KeyboardInput.SendEnter(window);
				Task.Delay(1000).Wait();
				KeyboardInput.SendEnter(window);
				return true;
			}
			return false;
		}

		private static bool MarkAllNoVoltorbs(Board board, nint window)
		{
			for (int x = 0; x < 5; x++)
			{
				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual == null && !tile.PossibeValues.HasFlag(PosibleValues.Bomb))
					{
						SetTile(board.Location, x, y, window);
						board.Location = (x, y);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// If all remaining tiles in row/col are bombs, mark them.
		/// </summary>
		/// <param name="board"></param>
		static void MarkBombs(Board board)
		{
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Bombs == actualHint.Tiles)
				{
					for (int x = 0; x < 5; x++)
					{
						if (board.Tiles[x, y].Actual == null)
							board.Tiles[x, y].Actual = 0;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Bombs == actualHint.Tiles)
				{
					for (int y = 0; y < 5; y++)
					{
						if (board.Tiles[x, y].Actual == null)
							board.Tiles[x, y].Actual = 0;
					}
				}
			}
		}

		/// <summary>
		/// Mark all rows/cols with 0 voltorbs or coins
		/// </summary>
		/// <param name="board"></param>
		/// <param name="window"></param>
		static bool Stage1(Board board, IntPtr window)
		{
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Bombs == 0)
				{
					for (int x = 0; x < 5; x++)
					{
						if (board.Tiles[x, y].Actual != null)
							continue;

						SetTile(board.Location, x, y, window);
						board.Location = (x, y);
						return true;
					}
				}
				else if (actualHint.Bombs == actualHint.Tiles)
				{
					for (int x = 0; x < 5; x++)
					{
						if (board.Tiles[x, y].Actual != null)
							continue;

						board.Tiles[x, y].Actual = 0;
						return true;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Bombs == 0)
				{
					for (int y = 0; y < 5; y++)
					{
						if (board.Tiles[x, y].Actual != null)
							continue;

						SetTile(board.Location, x, y, window);
						board.Location = (x, y);
						return true;
					}
				}
				if (actualHint.Bombs == actualHint.Tiles)
				{
					for (int y = 0; y < 5; y++)
					{
						if (board.Tiles[x, y].Actual != null)
							continue;

						board.Tiles[x, y].Actual = 0;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Memo all rows that add up to 5.
		/// </summary>
		/// <param name="board"></param>
		static bool Stage2(Board board)
		{
			var hasChanged = false;
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Coins + actualHint.Bombs == actualHint.Tiles)
				{
					for (int x = 0; x < 5; x++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null)
							continue;

						if (tile.PossibeValues.HasFlag(PosibleValues.Three))
						{
							tile.PossibeValues &= ~PosibleValues.Three;
							hasChanged = true;
						}
						if (tile.PossibeValues.HasFlag(PosibleValues.Two))
						{
							tile.PossibeValues &= ~PosibleValues.Two;
							hasChanged = true;
						}
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				if (actualHint.Coins + actualHint.Bombs == actualHint.Tiles)
				{
					for (int y = 0; y < 5; y++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null)
							continue;

						if (tile.PossibeValues.HasFlag(PosibleValues.Three))
						{
							tile.PossibeValues &= ~PosibleValues.Three;
							hasChanged = true;
						}
						if (tile.PossibeValues.HasFlag(PosibleValues.Two))
						{
							tile.PossibeValues &= ~PosibleValues.Two;
							hasChanged = true;
						}
					}
				}
			}
			return hasChanged;
		}

		/// <summary>
		/// Memo any rows with 4 voltorbs.
		/// </summary>
		/// <param name="board"></param>
		static bool Stage3(Board board)
		{
			var hasChanged = false;
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0 ||
					actualHint.Bombs + 1 != actualHint.Tiles)
					continue;

				for (int x = 0; x < 5; x++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					if (actualHint.Coins != 1 && tile.PossibeValues.HasFlag(PosibleValues.One))
					{
						tile.PossibeValues &= ~PosibleValues.One;
						hasChanged = true;
					}
					if (actualHint.Coins != 2 && tile.PossibeValues.HasFlag(PosibleValues.Two))
					{
						tile.PossibeValues &= ~PosibleValues.Two;
						hasChanged = true;
					}
					if (actualHint.Coins != 3 && tile.PossibeValues.HasFlag(PosibleValues.Three))
					{
						tile.PossibeValues &= ~PosibleValues.Three;
						hasChanged = true;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0 ||
					actualHint.Bombs + 1 != actualHint.Tiles)
					continue;

				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					if (actualHint.Coins != 1 && tile.PossibeValues.HasFlag(PosibleValues.One))
					{
						tile.PossibeValues &= ~PosibleValues.One;
						hasChanged = true;
					}
					if (actualHint.Coins != 2 && tile.PossibeValues.HasFlag(PosibleValues.Two))
					{
						tile.PossibeValues &= ~PosibleValues.Two;
						hasChanged = true;
					}
					if (actualHint.Coins != 3 && tile.PossibeValues.HasFlag(PosibleValues.Three))
					{
						tile.PossibeValues &= ~PosibleValues.Three;
						hasChanged = true;
					}
				}
			}
			return hasChanged;
		}

		/// <summary>
		/// Rule out 3s for any row that totals 6
		/// </summary>
		/// <param name="board"></param>
		static bool Stage4(Board board)
		{
			var hasChanged = false;
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0 ||
					actualHint.Bombs + actualHint.Coins != actualHint.Tiles + 1)
					continue;

				for (int x = 0; x < 5; x++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					if (tile.PossibeValues.HasFlag(PosibleValues.Three))
					{
						tile.PossibeValues &= ~PosibleValues.Three;
						hasChanged = true;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0 ||
					actualHint.Bombs + actualHint.Coins != actualHint.Tiles + 1)
					continue;

				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					if (tile.PossibeValues.HasFlag(PosibleValues.Three))
					{
						tile.PossibeValues &= ~PosibleValues.Three;
						hasChanged = true;
					}
				}
			}
			return hasChanged;
		}

		/// <summary>
		/// To high for 1s
		/// </summary>
		/// <param name="board"></param>
		static bool Stage5(Board board)
		{
			var hasChanged = false;
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				var numberedSquars = actualHint.Tiles - actualHint.Bombs;
				if (numberedSquars == 1 ||
					(numberedSquars == 2 && actualHint.Coins < 5) ||
					(numberedSquars == 3 && actualHint.Coins < 8) ||
					(numberedSquars == 4 && actualHint.Coins < 11) ||
					numberedSquars == 5)
					continue;

				for (int x = 0; x < 5; x++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;
					
					if (tile.PossibeValues.HasFlag(PosibleValues.One))
					{
						tile.PossibeValues &= ~PosibleValues.One;
						hasChanged = true;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				var numberedSquars = actualHint.Tiles - actualHint.Bombs;
				if (numberedSquars == 1 ||
					(numberedSquars == 2 && actualHint.Coins < 5) ||
					(numberedSquars == 3 && actualHint.Coins < 8) ||
					(numberedSquars == 4 && actualHint.Coins < 11) ||
					numberedSquars == 5)
					continue;

				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
						continue;

					if (tile.PossibeValues.HasFlag(PosibleValues.One))
					{
						tile.PossibeValues &= ~PosibleValues.One;
						hasChanged = true;
					}
				}
			}
			return hasChanged;
		}

		/// <summary>
		/// Additional rules
		/// </summary>
		static bool Stage6(Board board, IntPtr window)
		{
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				var numberedSquars = actualHint.Tiles - actualHint.Bombs;

				int atLeastOnes = 0;
				int atLeastTwos = 0;
				int atLeastThrees = 0;
				if (actualHint.Coins < 2 * numberedSquars)
					atLeastOnes = 2 * numberedSquars - actualHint.Coins;
				if (actualHint.Coins > 2 * numberedSquars)
					atLeastThrees = actualHint.Coins - 2 * numberedSquars;
				if (actualHint.Coins % 2 != numberedSquars % 2)
					atLeastTwos = 1;

				var memoOnes = 0;
				var memoTwos = 0;
				var memoThrees = 0;
				for (int x = 0; x < 5; x++)
				{
					var tile = board.Tiles[x, y];

					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.One))
						memoOnes++;
					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.Two))
						memoTwos++;
					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.Three))
						memoThrees++;
				}

				if (atLeastOnes > 0 && atLeastOnes == memoOnes)
				{
					for (int x = 0; x < 5; x++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.One))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}

				if (atLeastTwos > 0 && atLeastTwos == memoTwos)
				{
					for (int x = 0; x < 5; x++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.Two))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}

				if (atLeastThrees > 0 && atLeastThrees == memoThrees)
				{
					for (int x = 0; x < 5; x++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.Three))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				var numberedSquars = actualHint.Tiles - actualHint.Bombs;

				int atLeastOnes = 0;
				int atLeastTwos = 0;
				int atLeastThrees = 0;
				if (actualHint.Coins < 2 * numberedSquars)
					atLeastOnes = 2 * numberedSquars - actualHint.Coins;
				if (actualHint.Coins > 2 * numberedSquars)
					atLeastThrees = actualHint.Coins - 2 * numberedSquars;
				if (actualHint.Coins % 2 != numberedSquars % 2)
					atLeastTwos = 1;

				var memoOnes = 0;
				var memoTwos = 0;
				var memoThrees = 0;
				for (int y = 0; y < 5; y++)
				{
					var tile = board.Tiles[x, y];

					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.One))
						memoOnes++;
					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.Two))
						memoTwos++;
					if (tile.Actual == null && tile.PossibeValues.HasFlag(PosibleValues.Three))
						memoThrees++;
				}

				if (atLeastOnes > 0 && atLeastOnes == memoOnes)
				{
					for (int y = 0; y < 5; y++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.One))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}

				if (atLeastTwos > 0 && atLeastTwos == memoTwos)
				{
					for (int y = 0; y < 5; y++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.Two))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}

				if (atLeastThrees > 0 && atLeastThrees == memoThrees)
				{
					for (int y = 0; y < 5; y++)
					{
						var tile = board.Tiles[x, y];
						if (tile.Actual != null ||
							!tile.PossibeValues.HasFlag(PosibleValues.Three))
							continue;

						SetTile(board.Location, x, y, window);
						board.Location.X = x;
						board.Location.Y = y;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Cannot be voltorb
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		static void Stage7(Board board)
		{
			for (int y = 0; y < 5; y++)
			{
				var actualHint = ActualHintX(board, y);
				if (actualHint.Tiles == 0)
					continue;

				List<Tile> tiles = [];
				for (int x = 0; x < 5; x++)
					if (board.Tiles[x, y].Actual == null)
						tiles.Add(board.Tiles[x, y]);
				var tileArray = tiles.ToArray();

				var validAssignments = new List<int[]>();
				GenerateAssignments(tileArray, actualHint, new int[tiles.Count], 0, 0, 0, validAssignments);
				var possibleAt = new HashSet<int>[5];
				for (int i = 0; i < tiles.Count; i++)
					possibleAt[i] = [];

				foreach (var assignment in validAssignments)
					for (int i = 0; i < tiles.Count; i++)
						possibleAt[i].Add(assignment[i]);

				// Eliminate impossible values
				for (int i = 0; i < tiles.Count; i++)
				{
					var mask = PosibleValues.None;
					foreach (var v in possibleAt[i])
						mask |= v switch
						{
							0 => PosibleValues.Bomb,
							1 => PosibleValues.One,
							2 => PosibleValues.Two,
							3 => PosibleValues.Three,
							_ => PosibleValues.None
						};

					tileArray[i].PossibeValues &= mask;
				}
			}
			for (int x = 0; x < 5; x++)
			{
				var actualHint = ActualHintY(board, x);
				if (actualHint.Tiles == 0)
					continue;

				List<Tile> tiles = [];
				for (int y = 0; y < 5; y++)
					if (board.Tiles[x, y].Actual == null)
						tiles.Add(board.Tiles[x, y]);
				var tileArray = tiles.ToArray();

				var validAssignments = new List<int[]>();
				GenerateAssignments(tileArray, actualHint, new int[tiles.Count], 0, 0, 0, validAssignments);
				var possibleAt = new HashSet<int>[5];
				for (int i = 0; i < tiles.Count; i++)
					possibleAt[i] = [];

				foreach (var assignment in validAssignments)
					for (int i = 0; i < tiles.Count; i++)
						possibleAt[i].Add(assignment[i]);

				// Eliminate impossible values
				for (int i = 0; i < tiles.Count; i++)
				{
					var mask = PosibleValues.None;
					foreach (var v in possibleAt[i])
						mask |= v switch
						{
							0 => PosibleValues.Bomb,
							1 => PosibleValues.One,
							2 => PosibleValues.Two,
							3 => PosibleValues.Three,
							_ => PosibleValues.None
						};

					tileArray[i].PossibeValues &= mask;
				}
			}
		}

		private static void GenerateAssignments(Tile[] row, Hint hint, int[] current, int index, int usedBombs, int coinSum, List<int[]> results)
		{
			if (index == row.Length)
			{
				if (usedBombs == hint.Bombs && coinSum == hint.Coins)
					results.Add((int[])current.Clone());
				return;
			}

			var tile = row[index];

			foreach (var value in GetPossibleValues(tile.PossibeValues))
			{
				int newBombs = usedBombs + (value == 0 ? 1 : 0);
				int newSum = coinSum + (value > 0 ? value : 0);

				if (newBombs > hint.Bombs) continue;
				if (newSum > hint.Coins) continue;

				// max coins left check (for pruning)
				int maxRemaining = (row.Length - index - 1) * 3;
				if (newSum + maxRemaining < hint.Coins) continue;

				current[index] = value;
				GenerateAssignments(row, hint, current, index + 1, newBombs, newSum, results);
			}
		}

		private static IEnumerable<int> GetPossibleValues(PosibleValues values)
		{
			if (values.HasFlag(PosibleValues.Bomb)) yield return 0;
			if (values.HasFlag(PosibleValues.One)) yield return 1;
			if (values.HasFlag(PosibleValues.Two)) yield return 2;
			if (values.HasFlag(PosibleValues.Three)) yield return 3;
		}

		static void ScanBoard(Board board, Bitmap bmp)
		{
			var yPixel = 12;
			for (var y = 0; y < 5; ++y)
			{
				var xPixel = 79;
				for (var x = 0; x < 5; x++)
				{
					var tile = board.Tiles[x, y];
					if (tile.Actual != null)
					{
						xPixel += 32;
						continue;
					}

					tile.Actual = NumberMapperHint.GetNumber(bmp, xPixel, yPixel, true);
					xPixel += 32;
				}
				yPixel += 32;
			}
		}

		static void SetTile((int X, int Y) location, int x, int y, IntPtr window)
		{
			while (location.X != x || location.Y != y)
			{
				if (location.X < x)
				{
					KeyboardInput.SendRight(window);
					location.X += 1;
				}
				else if (location.X > x)
				{
					KeyboardInput.SendLeft(window);
					location.X -= 1;
				}
				else if (location.Y < y)
				{
					KeyboardInput.SendDown(window);
					location.Y += 1;
				}
				else if (location.Y > y)
				{
					KeyboardInput.SendUp(window);
					location.Y -= 1;
				}
			}
			KeyboardInput.SendEnter(window);
		}

		static Hint ActualHintX(Board board, int y)
		{
			var hint = board.XHints[y];
			var coins = hint.Coins;
			var bombs = hint.Bombs;
			var tiles = 5;

			for (int x = 0; x < 5; x++)
			{
				var tile = board.Tiles[x, y];
				if (tile.Actual == null)
					continue;

				if (tile.Actual == 0)
					bombs--;
				else
					coins -= tile.Actual.Value;
				tiles--;
			}

			return new(coins, bombs, tiles);
		}

		static Hint ActualHintY(Board board, int x)
		{
			var hint = board.YHints[x];
			var coins = hint.Coins;
			var bombs = hint.Bombs;
			var tiles = 5;

			for (int y = 0; y < 5; y++)
			{
				var tile = board.Tiles[x, y];
				if (tile.Actual == null)
					continue;

				if (tile.Actual == 0)
					bombs--;
				else
					coins -= tile.Actual.Value;
				tiles--;
			}

			return new(coins, bombs, tiles);
		}
	}
}
