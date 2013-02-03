using System;
using System.Collections.Generic;

namespace ColorduinoMaster
{
	public class BarGraph
	{
		public BarGraph ()
		{
		}

		public IEnumerable<Frame> Render(float la, float lb, float lc)
		{
			var frame = new MutableFrame();

			byte r, g, b;
			
			r = (byte)(0xFF * la);
			g = (byte)(0xFF - r);
			b = 0x00;
			
			for (int i = 0; i < 8 * la; i++)
			{
				frame.SetPixel(0, i, r, g, b);
				frame.SetPixel(1, i, r, g, b);
			}
			
			r = (byte)(0xFF * lb);
			g = (byte)(0xFF - r);
			b = 0x00;
			
			for (int i = 0; i < 8 * lb; i++)
			{
				frame.SetPixel(3, i, r, g, b);
				frame.SetPixel(4, i, r, g, b);
			}
			
			r = (byte)(0xFF * lc);
			g = (byte)(0xFF - r);
			b = 0x00;
			
			for (int i = 0; i < 8 * lc; i++)
			{
				frame.SetPixel(6, i, r, g, b);
				frame.SetPixel(7, i, r, g, b);
			}

			return new Frame[] { frame };
		}
	}
}

