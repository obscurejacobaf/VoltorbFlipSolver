using System.Runtime.InteropServices;

namespace VoltorbFlipSolver
{
	public static class KeyboardInput
	{
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);

		// Virtual key codes
		private const int VK_LEFT = 0x25;
		private const int VK_UP = 0x26;
		private const int VK_RIGHT = 0x27;
		private const int VK_DOWN = 0x28;
		private const int VK_RETURN = 0x0D;

		public static void SendKey(IntPtr hWnd, int virtualKey)
		{
			uint scanCode = MapVirtualKey((uint)virtualKey, 0); // MAPVK_VK_TO_VSC
			IntPtr lParamDown = (IntPtr)(1 | (scanCode << 16)); // repeat count = 1, scan code in bits 16-23
			IntPtr lParamUp = (IntPtr)(1 | (scanCode << 16) | (1 << 30) | (1 << 31)); // release

			SendMessage(hWnd, WM_KEYDOWN, (IntPtr)virtualKey, lParamDown);
			Task.Delay(50).Wait();
			SendMessage(hWnd, WM_KEYUP, (IntPtr)virtualKey, lParamUp);
			Task.Delay(50).Wait();
		}

		public static void SendLeft(IntPtr hWnd) => SendKey(hWnd, VK_LEFT);
		public static void SendRight(IntPtr hWnd) => SendKey(hWnd, VK_RIGHT);
		public static void SendUp(IntPtr hWnd) => SendKey(hWnd, VK_UP);
		public static void SendDown(IntPtr hWnd) => SendKey(hWnd, VK_DOWN);
		public static void SendEnter(IntPtr hWnd) => SendKey(hWnd, VK_RETURN);
	}
}
