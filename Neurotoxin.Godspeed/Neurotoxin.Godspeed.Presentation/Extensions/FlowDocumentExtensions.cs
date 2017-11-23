using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class FlowDocumentExtensions
    {
        public static IEnumerable<T> GetElementsOfType<T>(this FlowDocument doc) where T : DependencyObject
        {
            return GetElementsOfType<T>((DependencyObject) doc);
        }

        private static IEnumerable<T> GetElementsOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            foreach (var c in LogicalTreeHelper.GetChildren(parent))
            {
                if (c is T)
                {
                    yield return (T) c;
                } 
                else if (c is DependencyObject)
                {
                    foreach (var cc in GetElementsOfType<T>((DependencyObject)c))
                    {
                        yield return cc;
                    }    
                }
            }
        }
    }
}