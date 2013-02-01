using System;
using System.Drawing;

namespace ColorduinoMaster
{
	public class Frame
	{
		public int Duration { get; set; }

		public byte[] Data { get; set; }

		public Frame()
		{
			Duration = 0;
			Data = new byte[64 * 3];
		}

		public void SetPixel(int x, int y, int c)
		{
			SetPixel(x, y,
			         (byte)((c >> 16) & 0xFF),
			         (byte)((c >> 8) & 0xFF),
			         (byte)(c & 0xFF));
		}
		
		public void SetPixel(int x, int y, byte r, byte g, byte b)
		{
			int index = PosToIndex(x, y);
			Data[index++] = r;
			Data[index++] = g;
			Data[index] = b;
		}

		private int PosToIndex(int x, int y)
		{
			return (x * 8 + y) * 3;
		}

		public void LoadPng(string filename)
		{
			using (Bitmap bitmap = new Bitmap(filename)) {
				int width = Math.Min (bitmap.Width, 8);
				int height = Math.Min (bitmap.Height, 8);
				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						var c = bitmap.GetPixel (x, y);
						SetPixel (x, y, c.R, c.G, c.B);
					}
				}
			}
		}
	}
}

