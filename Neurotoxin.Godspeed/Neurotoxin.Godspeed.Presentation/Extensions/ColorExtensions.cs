using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class ColorExtensions
    {
        public static Color[] GetRandomPalette(this Color mix, int count)
        {
            var random = new Random();
            var colors = new Color[count];

            for(var i = 0; i < count; i++)
            {
                var red = random.Next(256);
                var green = random.Next(256);
                var blue = random.Next(256);

                red = (red + mix.R) / 2;
                green = (green + mix.G) / 2;
                blue = (blue + mix.B) / 2;

                colors[i] = Color.FromRgb((byte)red, (byte)green, (byte)blue);
            }

            return colors;
        }
    }
}
