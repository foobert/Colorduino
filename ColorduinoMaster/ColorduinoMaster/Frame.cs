using System;

namespace ColorduinoMaster
{
	public struct Frame
	{
		public int Duration { get; set; }

		public byte[] Data { get; set; }

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

	}
}

