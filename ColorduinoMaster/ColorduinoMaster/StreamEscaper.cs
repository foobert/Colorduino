using System;
using System.IO;

namespace ColorduinoMaster
{
	public class StreamEscaper
	{
		public byte[] Escape(byte msgId, byte[] data)
		{
			ushort sum1 = 0;
			ushort sum2 = 0;
			
			MemoryStream mem = new MemoryStream();
			
			// write start byte
			mem.WriteByte(0x7F);
			AppendByte(mem, msgId, ref sum1, ref sum2);

			for (int i = 0; i < data.Length; i++)
			{
				AppendByte(mem, data[i], ref sum1, ref sum2);
			}
			
			// write checksum bytes
			mem.WriteByte((byte)(sum2 & 0xFF));
			mem.WriteByte((byte)(sum1 & 0xFF));
			
			// write end byte
			mem.WriteByte(0x7E);

			return mem.ToArray();
		}

		private void AppendByte(MemoryStream stream, byte b, ref ushort sum1, ref ushort sum2)
		{
			// special bytes need escaping
			if (b == 0x7D || b == 0x7E || b == 0x7F)
			{
				stream.WriteByte(0x7D); // escape byte
				stream.WriteByte((byte)(b ^ 0x20)); // escape with static xor
			}
			else
			{
				stream.WriteByte(b);
			}
			
			// update checksum
			sum1 = (ushort)((sum1 + b) % 0xFF);
			sum2 = (ushort)((sum2 + sum1) % 0xFF);
		}
	}
}

