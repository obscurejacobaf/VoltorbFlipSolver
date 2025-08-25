using System.Drawing;
using System.Runtime.InteropServices;

namespace VoltorbFlipSolver
{
	public static class DwmWindowCapture
	{
		// ===== Public API =====

		/// <summary>
		/// Capture the window (without borders) by its title using DWM frame bounds.
		/// </summary>
		public static IntPtr GetWindowByTitle(string windowTitle)
		{
			IntPtr hWnd = FindWindowW(null, windowTitle);
			if (hWnd == IntPtr.Zero)
				throw new InvalidOperationException($"Window not found: \"{windowTitle}\"");

			return hWnd;
		}

		/// <summary>
		/// Capture the given window handle (without borders) using DWM frame bounds.
		/// </summary>
		public static Bitmap CaptureWindow(IntPtr hWnd)
		{
			EnsureDpiAwareness();
			if (hWnd == IntPtr.Zero) throw new ArgumentException("hWnd is null.");

			// Make sure it's not minimized and has had time to redraw if we restored it.
			RestoreIfMinimized(hWnd);

			RECT rect = GetClientBoundsScreen(hWnd);
			int width = rect.Right - rect.Left;
			int height = rect.Bottom - rect.Top;
			if (width <= 0 || height <= 0)
				throw new InvalidOperationException("Computed capture bounds are empty.");

			var bmp = new Bitmap(width, height);
			using (var g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
			}
			return AutoCropBlackBorders(bmp);
		}

		// ===== Internals =====

		private static RECT GetClientBoundsScreen(IntPtr hWnd)
		{
			if (!GetClientRect(hWnd, out RECT client))
				throw new InvalidOperationException("GetClientRect failed.");

			var pt = new POINT { X = 0, Y = 0 };
			if (!ClientToScreen(hWnd, ref pt))
				throw new InvalidOperationException("ClientToScreen failed.");

			// Convert to screen coordinates
			return new RECT
			{
				Left = pt.X,
				Top = pt.Y,
				Right = pt.X + (client.Right - client.Left),
				Bottom = pt.Y + (client.Bottom - client.Top)
			};
		}

		private static void RestoreIfMinimized(IntPtr hWnd)
		{
			if (IsIconic(hWnd))
			{
				ShowWindow(hWnd, SW_RESTORE);
				// Give DWM a moment to redraw the restored window
			}
			BringWindowToTop(hWnd);
			SetForegroundWindow(hWnd);
			Thread.Sleep(50);
		}

		public static Bitmap AutoCropBlackBorders(Bitmap bmp)
		{
			var left = 0;
			var top = 0;
			var right = bmp.Width - 1;
			var bottom = bmp.Height - 1;

			// Crop top
			for (int y = 0; y < bmp.Height; y++)
			{
				bool nonBlack = false;
				for (int x = 0; x < bmp.Width; x++)
				{
					Color c = bmp.GetPixel(x, y);
					if (c.R + c.G + c.B > 10)
					{
						nonBlack = true;
						break;
					}
				}
				if (nonBlack)
				{
					top = y;
					break;
				}
			}

			// Crop bottom
			for (int y = bmp.Height - 1; y >= 0; y--)
			{
				bool nonBlack = false;
				for (int x = 0; x < bmp.Width; x++)
				{
					Color c = bmp.GetPixel(x, y);
					if (c.R + c.G + c.B > 200)
					{
						nonBlack = true;
						break;
					}
				}
				if (nonBlack)
				{
					bottom = y;
					break;
				}
			}

			// Crop left
			for (int x = 0; x < bmp.Width; x++)
			{
				bool nonBlack = false;
				for (int y = 0; y < bmp.Height; y++)
				{
					Color c = bmp.GetPixel(x, y);
					if (c.R + c.G + c.B > 10)
					{
						nonBlack = true;
						break;
					}
				}
				if (nonBlack)
				{
					left = x;
					break;
				}
			}

			// Crop right
			for (int x = bmp.Width - 1; x >= 0; x--)
			{
				bool nonBlack = false;
				for (int y = 0; y < bmp.Height; y++)
				{
					Color c = bmp.GetPixel(x, y);
					if (c.R + c.G + c.B > 10)
					{
						nonBlack = true;
						break;
					}
				}
				if (nonBlack)
				{
					right = x;
					break;
				}
			}

			int width = right - left + 1;
			int height = bottom - top + 1;

			if (width <= 0 || height <= 0)
				throw new InvalidOperationException("No non-black pixels found to crop.");

			return bmp.Clone(new Rectangle(left, top, width, height), bmp.PixelFormat);
		}

		// ===== DPI Awareness =====

		private static bool _dpiSet = false;
		private static void EnsureDpiAwareness()
		{
			if (_dpiSet) return;
			try
			{
				// Try the best option first (Windows 10+)
				if (SetProcessDpiAwarenessContext(new IntPtr(-4))) // PER_MONITOR_AWARE_V2
				{
					_dpiSet = true;
					return;
				}
			}
			catch
			{
				throw new InvalidOperationException();
			}
		}

		// ===== P/Invoke =====

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(
			IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_RESTORE = 9;

		// DPI Awareness
		[DllImport("user32.dll")]
		private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

		[DllImport("Shcore.dll")]
		private static extern int SetProcessDpiAwareness(int value);

		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		// Structs
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public int Width => Right - Left;
			public int Height => Bottom - Top;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
		}
	}
}
