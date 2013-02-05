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
				var font = new Font("font.png");
                var bars = new BarGraph();

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
						var filenames = input.Split(' ').Skip(1);
						var frames = filenames.Select(f => Frame.LoadImage(f));
						master.Animate(frames);
					}
					else if (input.StartsWith("gif "))
					{
						var filename = input.Substring(4);
						var frames = Frame.LoadGif(filename).ToArray();
                        var frames2 = bars.Overlay(frames, 0.9f, 0.5f, 0.3f);
//						foreach (var f in frames)
//						{
//							font.Render(f, "12", 0, 1, Color.Red);
//						}
						master.Animate(frames2);
					}
                    else if (input == "weather")
                    {
						weather.Refresh();
						var condition = weather.CurrentWeatherCondition;
						Console.WriteLine("Current weather: {0}", condition);
						if (File.Exists(condition + ".gif"))
						{
    						var frames = Frame.LoadGif(condition + ".gif");
                            int temp = weather.CurrentTemperature;
                            frames = font.Overlay(frames, temp.ToString(), temp < 10 ? 5 : 1, 2, Color.Red);
    						master.Animate(frames);
						}
                    }
					else if (input == "test")
					{
						var frame = new MutableFrame();
						frame.SetPixel(0, 0, 255, 0, 0);
						frame.SetPixel(0, 1, 0, 255, 0);
						frame.SetPixel(0, 2, 0, 0, 255);
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
						font.Render(frame, text, 0, 0, Color.Red);
						master.Animate(new Frame[] { frame });
					}
					else if (input.StartsWith("fill "))
					{
						var colors = input.Split(' ').Skip(1).Select(i => int.Parse(i)).ToArray();
						var color = Color.FromArgb(colors[0], colors[1], colors[2]);
						var frame = new MutableFrame();
						for (int i = 0; i < 8; i++)
							for (int j = 0; j < 8; j++)
								frame.SetPixel(i, j, color);
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
