using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;

namespace ColorduinoMaster
{
    internal class Master : IDisposable
    {
        private SerialPort _serial;
        private int _msgId;
        private Thread _readThread;
        private bool _running;
        private AutoResetEvent _readEvent;
        private volatile int _lastAck;

        public Master(string port)
        {
            _serial = new SerialPort(port, 9600);
            _serial.Open();
            _serial.DtrEnable = true;
            _readEvent = new AutoResetEvent(false);
            _lastAck = -1;
            _running = true;
            _readThread = new Thread(ReadLoop);
            _readThread.Start();
        }

        private void ReadLoop()
        {
            while (_running)
            {
                try
                {
                    byte ack = (byte)_serial.ReadByte();
//                    Console.WriteLine("Received ack for msg " + ack);
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

        public void Dispose()
        {
            _running = false;
            if (_serial != null)
            {
                _serial.Close();
            }
            if (_readThread != null)
                _readThread.Join();
        }

        private void Write(byte[] buffer)
        {
            while (true)
            {
                WriteEscaped(buffer);
                if (!_readEvent.WaitOne(1000) || _lastAck != _msgId)
                {
                    Console.WriteLine("Failed to send {0}", _msgId);
                    Thread.Sleep(100);
                }
                else
                {
//                    Console.WriteLine("Success!");
                    break;
                }
            }
        }

        private void WriteEscaped(byte[] buffer)
        {
            ushort sum1 = 0;
            ushort sum2 = 0;
            
            MemoryStream mem = new MemoryStream();
            mem.WriteByte(0x7F);
            byte b = (byte)(++_msgId % 0xFF);
            if (b == 0x7D || b == 0x7E || b == 0x7F)
            {
                mem.WriteByte(0x7D);
                mem.WriteByte((byte)(b ^ 0x20));
            }
            else
            {
                mem.WriteByte(b);
            }
            sum1 = (ushort)((sum1 + b) % 255);
            sum2 = (ushort)((sum2 + sum1) % 255);
            
            for (int i = 0; i < buffer.Length; i++)
            {
                b = buffer [i];
                if (b == 0x7D || b == 0x7E || b == 0x7F)
                {
                    mem.WriteByte(0x7D);
                    mem.WriteByte((byte)(b ^ 0x20));
                }
                else
                {
                    mem.WriteByte(b);
                }
                sum1 = (ushort)((sum1 + b) % 255);
                sum2 = (ushort)((sum2 + sum1) % 255);
            }
            mem.WriteByte((byte)(sum2 & 0xFF));
            mem.WriteByte((byte)(sum1 & 0xFF));
            mem.WriteByte(0x7E);
            byte[] escaped = mem.ToArray();

            _serial.Write(escaped, 0, escaped.Length);
        }

        public void Animate(params string[] files)
        {
            WriteNewAnimation();
            foreach (var file in files)
            {
                WriteNewFrame();
                byte[] frame = File.ReadAllBytes(file + ".bin");
                WriteFrameData(frame);
            }
            WriteStartAnimation();
        }

        public void ShowBars(float la, float lb, float lc)
        {
            WriteNewAnimation();
            WriteNewFrame();

            byte[] buffer = new byte[64 * 3];

            byte r, g, b;

            r = (byte)(0xFF * la);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * la; i++)
            {
                SetPixel (buffer, 0, i, r, g, b);
                SetPixel (buffer, 1, i, r, g, b);
            }

            r = (byte)(0xFF * lb);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * lb; i++)
            {
                SetPixel (buffer, 3, i, r, g, b);
                SetPixel (buffer, 4, i, r, g, b);
            }

            r = (byte)(0xFF * lc);
            g = (byte)(0xFF - r);
            b = 0x00;

            for (int i = 0; i < 8 * lc; i++)
            {
                SetPixel (buffer, 6, i, r, g, b);
                SetPixel (buffer, 7, i, r, g, b);
            }

            WriteFrameData(buffer);

            WriteStartAnimation();
        }

        private int PosToIndex(int x, int y)
        {
            return (x * 8 + y) * 3;
        }

        private void SetPixel(byte[] buffer, int x, int y, int c)
        {
            SetPixel(buffer, x, y,
                    (byte)((c >> 16) & 0xFF),
                    (byte)((c >> 8) & 0xFF),
                    (byte)(c & 0xFF));
        }

        private void SetPixel(byte[] buffer, int x, int y, byte r, byte g, byte b)
        {
            int index = PosToIndex(x, y);
            buffer[index++] = r;
            buffer[index++] = g;
            buffer[index] = b;
        }

        private void WriteNewAnimation()
        {
            byte[] buffer = new byte[1];
            buffer [0] = (byte)0x01;
            Write(buffer);
        }

        private void WriteStartAnimation()
        {
            byte[] buffer = new byte[1];
            buffer [0] = (byte)0x04;
            Write(buffer);
        }

        private void WriteNewFrame()
        {
            byte[] buffer = new byte[3];
            buffer [0] = (byte)0x02;
            buffer [1] = (byte)0x00;
            buffer [2] = (byte)0xFF;
            Write(buffer);
        }

        void WriteFrameData(byte[] frame)
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
                buffer[0] = (byte)0x03;
                Array.Copy(frame, offset, buffer, 1, todo);
                offset += todo;
                Write (buffer);
            }
        }
    }
}
