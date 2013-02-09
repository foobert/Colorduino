using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;

namespace ColorduinoMaster
{

	public class MutableFrame : Frame
	{
        private double _alpha;

		public MutableFrame()
		{
            _alpha = 1.0;
		}

		public MutableFrame(Frame frame, double alpha)
			: base(frame.Duration, frame.AllPixels().ToArray())
		{
            _alpha = alpha;
		}

        public MutableFrame(Frame frame)
            : this(frame, 1.0)
        {
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
			SetPixel(x, y, Color.FromArgb(r, g, b));
		}

		public void SetPixel(int x, int y, Color c)
        {
            int index = PosToIndex(x, y);
            if (_alpha < 1.0)
            {
                var oldColor = _pixels [index];
                var newColor = Color.FromArgb((oldColor.R + c.R) / 2, (oldColor.G + c.G) / 2, (oldColor.B + c.B) / 2);
                _pixels[index] = newColor;
            }
            else
            {
                _pixels[index] = c;
            }
        }

		public void LoadBitmap(Bitmap bitmap)
		{
			int width = Math.Min (bitmap.Width, 8);
			int height = Math.Min (bitmap.Height, 8);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var c = bitmap.GetPixel (x, y);
					SetPixel(x, y, c.R, c.G, c.B);
				}
			}
		}

	}
}
