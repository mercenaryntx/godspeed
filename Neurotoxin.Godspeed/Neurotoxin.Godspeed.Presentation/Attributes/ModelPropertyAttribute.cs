using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Attributes 
{

    /// <summary>
    /// Put on a viewmodel property to request that when the model root object changes,
    /// the NotifyPropertyChangedOnUIThread should be called on the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ModelPropertyAttribute : Attribute {}
}