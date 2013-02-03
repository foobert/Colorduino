using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;

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
						var frames = Frame.LoadGif(input.Substring(4));
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
