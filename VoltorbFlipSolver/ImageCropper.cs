using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltorbFlipSolver
{
	public static class ImageCropper
	{
		/// <summary>
		/// Crops a rectangular region from an image and saves it to disk.
		/// </summary>
		public static void SaveRegion(Bitmap source, Rectangle region, string outputPath)
		{
			using Bitmap cropped = new Bitmap(region.Width, region.Height);
			using Graphics g = Graphics.FromImage(cropped);
			g.DrawImage(source,
				new Rectangle(0, 0, cropped.Width, cropped.Height),
				region,
				GraphicsUnit.Pixel);

			string dir = Path.GetDirectoryName(outputPath)!;
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			cropped.Save(outputPath);
		}
	}
}
