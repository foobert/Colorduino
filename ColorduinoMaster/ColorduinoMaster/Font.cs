using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace ColorduinoMaster
{
	public class Font
	{
		private Dictionary<char, byte[,]> _font;

		private char[] _lookup = new char[]
        {
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', '°', '-'
        };

		public Font(string filename)
		{
			_font = new Dictionary<char, byte[,]> ();
			using (Bitmap bitmap = new Bitmap(filename))
			{
				for (int i = 0; i < _lookup.Length; i++)
				{
					if (i * 4 + 3 > bitmap.Width)
						break;

					_font[_lookup[i]] = LoadChar(bitmap, i * 4);
//					Console.WriteLine("Loaded {0} at index {1}", _lookup[i], i * 4);
				}
			}
		}

		private byte[,] LoadChar(Bitmap bitmap, int offset)
		{
			byte[,] result = new byte[3, bitmap.Height];

			for (int col = 0; col < 3; col++) {
				for (int row = 0; row < bitmap.Height; row++) {
					var pixel = bitmap.GetPixel(offset + col, row);
					result [col, row] = (byte)((pixel.R + pixel.G + pixel.B == 0) ? 0 : 1);
				}
			}
			return result;
		}

        public IEnumerable<Frame> Overlay(IEnumerable<Frame> frames, string text, int x, int y, Color color)
        {
            return frames.Select(f => Render(f, text, x, y, color));
        }

		public Frame Render(Frame frame, string text, int x, int y, Color color)
		{
			var result = new MutableFrame(frame);
			var chars = text.ToCharArray();
			foreach (var c in chars)
			{
//				Console.WriteLine("Rendering {0} at {1},{2}", c, x, y);
				byte[,] data;
				if (!_font.TryGetValue(c, out data))
					continue;

				for (int col = data.GetLowerBound(0); col <= data.GetUpperBound(0); col++)
				{
					for (int row = data.GetLowerBound(1); row <= data.GetUpperBound(1); row++)
					{
						byte value = data[col, row];
						if (value != 0)
						{
							result.SetPixel(x + col, y + row, color);
						}
					}
				}
				x += data.GetLength(0) + 1;
			}
			return result;
		}
	}
}

