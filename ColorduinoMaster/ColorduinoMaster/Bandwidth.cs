using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ColorduinoMaster
{
    public class Bandwidth
    {
        private IBandwidthProvider _bandwidthProvider;
        private string _device;
        private SlidingAverage _rxAvg;
        private SlidingAverage _txAvg;
        private DateTime _lastTimestamp;

        public Bandwidth(IBandwidthProvider bandwidthProvider, string device)
        {
            _bandwidthProvider = bandwidthProvider;
            _device = device;
            _rxAvg = new SlidingAverage(30);
            _txAvg = new SlidingAverage(30);
        }

        public int RxKilobytesPerSecond
        {
            get;
            private set;
        }

        public int TxKilobytesPerSecond
        {
            get;
            private set;
        }
       
        public void Update()
        {
            if (_lastTimestamp != DateTime.MinValue)
            {
                var duration = DateTime.UtcNow - _lastTimestamp; 
                var data = _bandwidthProvider.GetBytes(_device);
                int currentRx = (int)(data.Item1 / duration.TotalSeconds);
                int currentTx = (int)(data.Item2 / duration.TotalSeconds);

                int rx = currentRx;
                int tx = currentTx;
                //int rx = _rxAvg.Add(currentRx);
                //int tx = _txAvg.Add(currentTx);

                RxKilobytesPerSecond = rx / 1024;
                TxKilobytesPerSecond = tx / 1024;

//                Console.WriteLine("RX: {0}, TX: {1}", rx, tx);
            }
            _lastTimestamp = DateTime.UtcNow;
        }
    }

    public interface IBandwidthProvider
    {
        IEnumerable<string> ListDevices();
        Tuple<int, int> GetBytes(string device);
    }

    public class LinuxBandwidthProvider : IBandwidthProvider
    {
        private string[] Read()
        {
            string[] input = File.ReadAllLines("/proc/net/dev");
            return input.Skip(2).ToArray();
        }

        public IEnumerable<string> ListDevices()
        {
            var input = Read();
            return input.Select(line => line.Trim().Split(':')[0]).ToArray();
        }

        public Tuple<int, int> GetBytes(string device)
        {
            var input = Read();
            var line = input.Select(l => l.Trim().Split(':')).First(l => l [0] == device);
            var fields = line [1].Split('\t');
            return new Tuple<int, int>(int.Parse(fields [0]), int.Parse(fields [8]));
        }
    }

    public class DummyBandwidthProvider : IBandwidthProvider
    {
        private int _targetRx;
        private int _targetTx;
        private DateTime _lastQuery;

        public DummyBandwidthProvider(int rx, int tx)
        {
            _targetRx = rx;
            _targetTx = tx;
            _lastQuery = DateTime.UtcNow;
        }

        public IEnumerable<string> ListDevices()
        {
            return new[] { "eth0" };
        }

        public Tuple<int, int> GetBytes(string device)
        {
            var duration = DateTime.UtcNow - _lastQuery;
            _lastQuery = DateTime.UtcNow;

            return new Tuple<int, int>((int)(_targetRx * duration.TotalSeconds), (int)(_targetTx * duration.TotalSeconds));
        }
    }

    public class SlidingAverage
    {
        private Queue<int> _values;
        private int _sum;
        private int _size;

        public SlidingAverage(int size)
        {
            _size = size;
            _values = new Queue<int>(_size);
        }

        public int Add(int value)
        {
            _values.Enqueue(value);
            _sum += value;
            if (_values.Count > _size)
            {
                _sum -= _values.Dequeue();
            }
            return _sum / _values.Count;
        }
    }
}

