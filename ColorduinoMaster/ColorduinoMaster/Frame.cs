using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections.Generic;

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

		public Color GetPixel(int x, int y)
		{
			int index = PosToIndex(x, y);
			return Color.FromArgb(Data[index++], Data[index++], Data[index]);
		}

		public IEnumerable<Color> AllPixels()
		{
			int index = 0;
			while (index < Data.Length)
			{
				yield return Color.FromArgb(Data[index++], Data[index++], Data[index++]);
			}
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
			return (x * 8 + 7 - y) * 3;
		}

		public void LoadPng(string filename)
		{
			using (Bitmap bitmap = new Bitmap(filename)) {
				LoadBitmap(bitmap);
			}
		}

		public static IEnumerable<Frame> LoadGif(string filename)
		{
			using (Bitmap bitmap = new Bitmap(filename)) {
				foreach (var d in bitmap.FrameDimensionsList)
				{
					Console.WriteLine("Dim: {0}, Count: {1}", d.ToString(), bitmap.GetFrameCount(new FrameDimension(d)));
				}

				FrameDimension dim = new FrameDimension(bitmap.FrameDimensionsList[0]);
				for (int i = 0; i < bitmap.GetFrameCount(dim); i++)
				{
					bitmap.SelectActiveFrame(dim, i);
					var frame = new Frame();
					frame.LoadBitmap(bitmap);
					yield return frame;
				}
			}
		}

		private void LoadBitmap(Bitmap bitmap)
		{
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

