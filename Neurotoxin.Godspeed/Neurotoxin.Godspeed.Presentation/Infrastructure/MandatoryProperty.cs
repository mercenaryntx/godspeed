using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class MandatoryProperty
    {
        public PropertyInfo Property { get; set; }
        public ValidationAttribute[] Validators { get; set; }
        public bool IsEnabled { get; set; }

        public MandatoryProperty(PropertyInfo property, ValidationAttribute[] validators, bool isEnabled = true)
        {
            Property = property;
            Validators = validators;
            IsEnabled = isEnabled;
        }
    }
}
