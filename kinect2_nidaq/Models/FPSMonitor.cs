using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class ConcurrentCircularBuffer<T>
    {
        private readonly LinkedList<T> _buffer;
        private int _maxItemCount;

        public ConcurrentCircularBuffer(int maxItemCount)
        {
            _maxItemCount = maxItemCount;
            _buffer = new LinkedList<T>();
        }

        public void Put(T item)
        {
            lock (_buffer)
            {
                _buffer.AddFirst(item);
                if (_buffer.Count > _maxItemCount)
                {
                    _buffer.RemoveLast();
                }
            }
        }

        public IEnumerable<T> Read()
        {
            lock (_buffer) { return _buffer.ToArray(); }
        }
    }

    class FPSMonitor
    {
        protected ConcurrentCircularBuffer<double> buffer;
        protected int num_samples;

        public FPSMonitor() : this(100) { }
        public FPSMonitor(int num_samples)
        {
            this.num_samples = num_samples;
            this.buffer = new ConcurrentCircularBuffer<double>(this.num_samples);
        }

        public void add_sample(double value)
        {
            this.buffer.Put(value);
        }

        public double calc_fps()
        {
            var items = this.buffer.Read();
            if (items.Count() > 2)
            {
                return Math.Round(1 / items.Zip(items.Skip(1), (a, b) => a - b).Average(), 0);
            }
            return 0;
        }
    }
}
