using System;
using System.Globalization;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Validation
{
    public class IsInteger : ValidationRule
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return string.IsNullOrEmpty(_errorMessage) ? "[{0}] is not an Integer value." : _errorMessage; }
            set { _errorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res;
            return value != null && Int32.TryParse(value.ToString(), out res)
                       ? new ValidationResult(true, null)
                       : new ValidationResult(false, string.Format(ErrorMessage, value));
        }
    }
}