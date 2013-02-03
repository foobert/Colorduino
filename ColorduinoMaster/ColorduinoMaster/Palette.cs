using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ColorduinoMaster
{
	public class Palette
	{
		private List<Color> _colors;

		public Palette(IEnumerable<Frame> frames)
		{
			_colors = frames.SelectMany(f => f.AllPixels()).Distinct().ToList();
		}

		public byte[] Data
		{
			get
			{
				var buffer = new byte[_colors.Count * 3];
				int index = 0;
				foreach (var color in _colors)
				{
					buffer [index++] = color.R;
					buffer [index++] = color.G;
					buffer [index++] = color.B;
				}
				return buffer;
			}
		}

		public byte[] EncodeFrame(Frame frame)
		{
			byte[] encoded = new byte[64];
			int index = 0;
			foreach (var color in frame.AllPixels())
			{
				encoded[index++] = (byte)_colors.IndexOf(color);
			}
			return encoded;
		}
	}
}

