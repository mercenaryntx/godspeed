using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Presentation.Events
{
    public delegate void ErrorCheckedEventHandler(object sender, ErrorCheckedEventArgs e);

    public class ErrorCheckedEventArgs : EventArgs
    {
        public string PropertyName { get; private set; }

        public ErrorCheckedEventArgs(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

}
