using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Finds the ancestor with the specified type.
        /// </summary>
        public static T FindAncestor<T>(this FrameworkElement f) where T : DependencyObject
        {
            while (true)
            {
                var p = VisualTreeHelper.GetParent(f) ?? f.Parent ?? f.TemplatedParent;
                if (p == null)
                    return null;
                if (p is T)
                    return (T)p;
                if (!(p is FrameworkElement))
                    return null;
                f = (FrameworkElement)p;
            }
        }

        public static IEnumerable<T> FindDescendants<T>(this FrameworkElement f) where T : FrameworkElement
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(f); i++)
            {
                var c = VisualTreeHelper.GetChild(f, i) as FrameworkElement;
                if (c is T)
                {
                    yield return (T)c;
                } 
                else
                {
                    foreach (var cc in FindDescendants<T>(c))
                    {
                        yield return cc;
                    }
                }
            }
        } 
    }
}