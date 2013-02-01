using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Collections.Generic;

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
            while (!sent)
            {
                WriteEscaped(buffer);
				sent = _readEvent.WaitOne(1000) && (_lastAck == _nextMessageId);
				if (!sent)
                {
                    Console.WriteLine("Failed to send {0}", _nextMessageId);
                    Thread.Sleep(100);
				}
            }
        }

        private void WriteEscaped(byte[] buffer)
        {
			byte msgId = (byte)(++_nextMessageId % 0xFF);
			byte[] escaped = _escaper.Escape(msgId, buffer);
            _serial.Write(escaped, 0, escaped.Length);
        }

		public void Animate(IEnumerable<Frame> frames)
		{
			WriteNewAnimation ();
			foreach (var frame in frames)
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
            if (frame.Length != 64 * 3)
                throw new ArgumentOutOfRangeException("invalid frame length " + frame.Length);

            while (offset < frame.Length)
            {
                byte todo = (byte)Math.Min(30, frame.Length - offset);
                if (todo % 3 != 0)
                    Console.WriteLine("invalid batch length");
                byte[] buffer = new byte[todo + 1];
                buffer[0] = CMD_APPEND_FRAME;
                Array.Copy(frame, offset, buffer, 1, todo);
                offset += todo;
                Write (buffer);
            }
        }
    }
}
