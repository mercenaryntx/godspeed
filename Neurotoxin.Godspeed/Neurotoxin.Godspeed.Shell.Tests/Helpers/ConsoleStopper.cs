using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Shell.Tests.Helpers
{
    public class ConsoleStopper : IDisposable
    {
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly string _text;

        public ConsoleStopper(string text = null)
        {
            _text = text;
            _sw.Start();
        }

        public void Dispose()
        {
            Console.WriteLine("{0}: {1}", _text ?? "Duration", _sw.Elapsed);
        }
    }
}
