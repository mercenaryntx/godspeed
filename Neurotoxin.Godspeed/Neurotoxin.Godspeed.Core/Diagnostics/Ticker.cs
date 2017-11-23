using System;
using System.Diagnostics;
using System.Timers;

namespace Neurotoxin.Godspeed.Core.Diagnostics
{
    public class Ticker
    {
        private readonly Timer _timer = new Timer { Enabled = false, Interval = 1000 };
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public TimeSpan Elapsed
        {
            get { return _stopwatch.Elapsed; }
        }

        public event ElapsedEventHandler Tick
        {
            add { _timer.Elapsed += value; }
            remove { _timer.Elapsed -= value; }
        }

        public void Start()
        {
            _timer.Start();
            _stopwatch.Start();
        }

        public void Restart()
        {
            _timer.Start();
            _stopwatch.Restart();
        }

        public void Stop()
        {
            _timer.Stop();
            _stopwatch.Stop();
        }

    }
}