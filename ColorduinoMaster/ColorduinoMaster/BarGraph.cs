using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace ColorduinoMaster
{
	public class BarGraph
	{
		public IEnumerable<Frame> Render(float la, float lb, float lc)
		{
			var frame = new Frame();
            Write (frame, la, lb, lc);
			return new Frame[] { frame };
		}

        public IEnumerable<Frame> Overlay(IEnumerable<Frame> frames, float la, float lb, float lc)
        {
            return frames.Select(f => Write(f, la, lb, lc));
        }

        private Frame Write(Frame frame, float la, float lb, float lc)
        {
            MutableFrame result = new MutableFrame(frame);
            byte r, g, b;

            r = (byte)(0xFF * la);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * la; i++)
            {
                result.SetPixel(0, 7 - i, r, g, b);
                result.SetPixel(1, 7 - i, r, g, b);
            }
            
            r = (byte)(0xFF * lb);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * lb; i++)
            {
                result.SetPixel(3, 7 - i, r, g, b);
                result.SetPixel(4, 7 - i, r, g, b);
            }
            
            r = (byte)(0xFF * lc);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * lc; i++)
            {
                result.SetPixel(6, 7 - i, r, g, b);
                result.SetPixel(7, 7 - i, r, g, b);
            }

            return result;
        }
	}
}

