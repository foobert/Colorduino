using System;
using System.Collections.Generic;
using System.IO;

namespace ColorduinoMaster
{
	public class FileAnimation
	{
		public IEnumerable<Frame> Render(params string[] files)
		{
			throw new NotImplementedException();

//			foreach (var file in files)
//			{
//				byte[] frame = File.ReadAllBytes(file + ".bin");
//				yield return new Frame { Duration = 1000, Data = frame };
//			}
		}
	}
}

