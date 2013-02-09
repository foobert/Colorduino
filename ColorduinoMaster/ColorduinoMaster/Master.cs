using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace ColorduinoMaster
{
    internal class Master : IDisposable
    {
		private const byte CMD_NEW_ANIMATION = 0x01;
		private const byte CMD_NEW_FRAME = 0x02;
		private const byte CMD_APPEND_FRAME = 0x03;
		private const byte CMD_START_ANIMATION = 0x04;
		private const byte CMD_FILL = 0x05;
		private const byte CMD_PLASMA = 0x06;
        private const byte CMD_APPEND_PALETTE = 0x07;

		private bool _running;

		private SerialPort _serial;
		private StreamEscaper _escaper;

		private int _nextMessageId;
        
		private Thread _readThread;
        private AutoResetEvent _readEvent;
        private volatile int _lastAck;


        public Master(string port)
        {
			_escaper = new StreamEscaper();
            _serial = new SerialPort(port, 9600);
            _serial.Open();
            _serial.DtrEnable = true;
            _readEvent = new AutoResetEvent(false);
            _lastAck = -1;
            _running = true;
            _readThread = new Thread(ReadLoop);
            _readThread.Start();
        }

        public void Dispose()
        {
            _running = false;
            if (_serial != null)
                _serial.Close();
            if (_readThread != null && Thread.CurrentThread != _readThread && _readThread.IsAlive)
                _readThread.Join();
        }

		private void ReadLoop()
		{
			while (_running)
			{
				try
				{
					byte ack = (byte)_serial.ReadByte();
					_lastAck = ack;
					_readEvent.Set();
				}
				catch (Exception)
				{
					if (!_running)
						break;
				}
			}
		}
		
		private void Write(byte[] buffer)
        {
			bool sent = false;
            while (_running && !sent)
            {
                var msgId = WriteEscaped(buffer);
                bool gotReadEvent = _readEvent.WaitOne(2000);
                if (!gotReadEvent)
                {
                    Console.WriteLine("ACK timeout");
                    sent = false;
                }
                else if (_lastAck != msgId)
                {
                    Console.WriteLine("ACK mismatch, expected {0} but got {1}", msgId, _lastAck);
                    sent = false;
                }
                else
                {
                    sent = true;
                }

				if (!sent)
                {
                    Console.WriteLine("Failed to send {0}", _nextMessageId);
                    Thread.Sleep(100);
				}
            }
        }

        private int WriteEscaped(byte[] buffer)
        {
			byte msgId = (byte)(++_nextMessageId % 0xFF);
			byte[] escaped = _escaper.Escape(msgId, buffer);
            _serial.Write(escaped, 0, escaped.Length);
            return msgId;
        }

		public void Animate(IEnumerable<Frame> frames)
		{
			var palette = new Palette(frames);
            var encodedFrames = frames.Select(f => new { Data = f.Encode(palette), Duration = f.Duration });

            int size = palette.Data.Length + encodedFrames.Sum(f => f.Data.Length);
            Console.WriteLine("Uploading animation of {0} frame(s) ({1} bytes)", frames.Count(), size);

			WriteNewAnimation();
            WritePalette(palette);
			foreach (var frame in encodedFrames)
			{
				WriteNewFrame(frame.Duration);
				WriteFrameData(frame.Data);
			}
			WriteStartAnimation();
		}


        private void WriteNewAnimation()
        {
            byte[] buffer = new byte[1];
            buffer [0] = CMD_NEW_ANIMATION;
            Write(buffer);
        }

        private void WritePalette(Palette palette)
        {
            int offset = 0;
            while (offset < palette.Data.Length)
            {
                byte todo = (byte)Math.Min(30, palette.Data.Length - offset);
                byte[] buffer = new byte[todo + 1];
                buffer[0] = CMD_APPEND_PALETTE;
                Array.Copy(palette.Data, offset, buffer, 1, todo);
                offset += todo;
                Write (buffer);
            }
        }

        private void WriteAppendPalette(byte[] palette)
        {
            byte[] buffer = new byte[1 + palette.Length];
            buffer [0] = CMD_APPEND_PALETTE;
            Array.Copy(palette, 0, buffer, 1, palette.Length);
            Write(buffer);
        }

        private void WriteStartAnimation()
        {
            byte[] buffer = new byte[1];
            buffer [0] = CMD_START_ANIMATION;
            Write(buffer);
        }

        private void WriteNewFrame(int duration)
        {
            byte[] buffer = new byte[3];
            buffer [0] = CMD_NEW_FRAME;
			// todo interpret duration
            buffer [1] = (byte)0x00;
            buffer [2] = (byte)0xFF;
            Write(buffer);
        }

        private void WriteFrameData(byte[] frame)
        {
            int offset = 0;
            while (offset < frame.Length)
            {
                byte todo = (byte)Math.Min(30, frame.Length - offset);
                byte[] buffer = new byte[todo + 1];
                buffer[0] = CMD_APPEND_FRAME;
                Array.Copy(frame, offset, buffer, 1, todo);
                offset += todo;
				Write (buffer);
            }
        }

		public void WritePlasma()
		{
			byte[] buffer = new byte[1];
			buffer[0] = CMD_PLASMA;
			Write (buffer);
		}
    }
}
