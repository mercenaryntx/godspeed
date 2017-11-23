using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Core
{
    //HACK
    public static class LogHelper
    {
        public static void LogDuration(string text, Action action)
        {
            var before = DateTime.Now;
            action.Invoke();
            var diff = DateTime.Now - before;
            Debug.WriteLine("{0}: {1} sec", text, diff.TotalSeconds);
        }

        public static event EventHandler<ValueChangedEventArgs> StatusBarMax;
        public static event EventHandler<ValueChangedEventArgs> StatusBarChange;
        public static event EventHandler<TextChangedEventArgs> StatusBarText;

        public static void NotifyStatusBarMax(int count)
        {
            var e = StatusBarMax;
            if (e != null) StatusBarMax.Invoke(null, new ValueChangedEventArgs(count));
        }

        public static void NotifyStatusBarChange(int i)
        {
            var e = StatusBarChange;
            if (e != null) StatusBarChange.Invoke(null, new ValueChangedEventArgs(i));
        }

        public static void NotifyStatusBarText(string text)
        {
            var e = StatusBarText;
            if (e != null) StatusBarText.Invoke(null, new TextChangedEventArgs(text));
        }
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public readonly int NewValue;

        public ValueChangedEventArgs(int newValue)
        {
            NewValue = newValue;
        }
    }

    public class TextChangedEventArgs : EventArgs
    {
        public readonly string Text;

        public TextChangedEventArgs(string text)
        {
            Text = text;
        }
    }
}