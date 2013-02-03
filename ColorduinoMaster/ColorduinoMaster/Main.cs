using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;
using System.Drawing;

namespace ColorduinoMaster
{
    class MainClass
    {
        public static void Main(string[] args)
		{
			string dev = args[0];
			string lat = args[1];
			string lon = args[2];
			string api = args[3];

            using (var master = new Master(dev))
            {
				var weather = new Weather(lat, lon, api);

                while (true)
                {
                    Console.WriteLine("ready");
                    string input = Console.ReadLine();
                    if (input.StartsWith("bars "))
                    {
                        var floats = input.Split(' ').Skip(1).Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
						var frames = new BarGraph().Render(floats[0], floats[1], floats[2]);
						master.Animate(frames);
                    }
					else if (input.StartsWith("file "))
					{
						var filenames = input.Split(' ').Skip(1).Select(f => {
							var frame = new Frame() { Duration = 1000 };
							Console.WriteLine("Loading {0}", f);
							frame.LoadPng(f);
							return frame;});
						master.Animate(filenames);
					}
					else if (input.StartsWith("gif "))
					{
						var font = new Font();
						var frames = Frame.LoadGif(input.Substring(4)).ToArray();
						foreach (var f in frames)
						{
							font.Render(f, "12", 0, 1, Color.Red);
						}
						master.Animate(frames);
					}
                    else if (input == "weather")
                    {
						weather.Refresh();
						var condition = weather.CurrentWeatherCondition;
						Console.WriteLine("Current weather: {0}", condition);
						if (File.Exists(condition + ".gif"))
						{
						var frames = Frame.LoadGif(condition + ".gif");
						master.Animate(frames);
						}
                    }
					else if (input == "test")
					{
						var frame = new Frame();
						frame.SetPixel(0, 0, 255, 0, 0);
						frame.SetPixel(0, 1, 0, 255, 0);
						frame.SetPixel(0, 2, 0, 0, 255);
						var font = new Font();
						font.Render(frame, "12", 0, 0, Color.Red);
//						frame.Pixels[0] = Color.Red;
//						frame.Pixels[1] = Color.Green;
//						frame.Pixels[2] = Color.Blue;
						master.Animate(new Frame[] { frame });
					}
					else if (input.StartsWith("text "))
					{
						var text = input.Substring(5);
						var frame = new Frame();
						var font = new Font();
						font.Render(frame, text, 0, 0, Color.Red);
						master.Animate(new Frame[] { frame });
					}
					else if (input.StartsWith("fill "))
					{
						var colors = input.Split(' ').Skip(1).Select(i => int.Parse(i)).ToArray();
						var color = Color.FromArgb(colors[0], colors[1], colors[2]);
						var frame = new Frame();
						for (int i = 0; i < 64; i++)
							frame.Pixels[i] = color;
						master.Animate(new Frame[] { frame });
					}
					else if (input == "plasma")
					{
						master.WritePlasma();
					}
                    else if (input == "exit")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("invalid command");
                    }
                }
            }
            Console.WriteLine("goodbye");
        }
    }

}
