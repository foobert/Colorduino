using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;

// 5739e39925170135130501

namespace ColorduinoMaster
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			string dev = args[0];

            using (var master = new Master(dev))
            {
                while (true)
                {
                    Console.WriteLine("ready");
                    string input = Console.ReadLine();
                    if (input.StartsWith("bars "))
                    {
                        var floats = input.Split(' ').Skip(1).Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                        master.ShowBars(floats[0], floats[1], floats[2]);
                    }
                    else if (input == "test")
                    {
                        master.Animate("frame-a", "frame-b", "frame-c", "frame-e");
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
