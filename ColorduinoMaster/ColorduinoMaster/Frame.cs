using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;

namespace ColorduinoMaster
{
	public class Frame
	{
		protected readonly int _duration;
		protected readonly Color[] _pixels;

		public int Duration { get { return _duration; } }

		public Frame()
		{
			_duration = 1000; /* msec */
			_pixels = new Color[64];
		}

		protected Frame(int duration, Color[] pixels)
		{
			_duration = duration;
			_pixels = pixels;
		}

		public Color GetPixel(int x, int y)
		{
			int index = PosToIndex(x, y);
			return _pixels[index];
		}

		public IEnumerable<Color> AllPixels()
		{
			return _pixels;
		}

		protected int PosToIndex(int x, int y)
		{
			return x * 8 + y;
		}

		public static Frame LoadImage(string filename)
		{
			using (Bitmap bitmap = new Bitmap(filename))
			{
				MutableFrame frame = new MutableFrame();
				frame.LoadBitmap(bitmap);
				return frame;
			}
		}

		public static IEnumerable<Frame> LoadGif(string filename)
		{
			using (Bitmap bitmap = new Bitmap(filename))
			{
				FrameDimension dim = new FrameDimension(bitmap.FrameDimensionsList[0]);
				for (int i = 0; i < bitmap.GetFrameCount(dim); i++)
				{
					bitmap.SelectActiveFrame(dim, i);

					var frame = new MutableFrame();
					frame.LoadBitmap(bitmap);
					yield return frame;
				}
			}
		}

        public byte[] Encode(Palette palette)
        {
            return RLE(palette.EncodeFrame(this));
        }

        private byte[] RLE(byte[] data)
        {
            List<byte> mem = new List<byte>();
            byte last = 255;
            byte count = 0;
            foreach (var b in data)
            {
                if (b != last)
                {
                    if (count > 0)
                    {
                        mem.Add(count);
                        mem.Add(last);
                    }
                    count = 0;
                }
                last = b;
                count++;
            }
            if (count > 0)
            {
                mem.Add(count);
                mem.Add(last);
            }
            return mem.ToArray();
        }
	}

}