using System.Globalization;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Validation
{
    public class IsRequired : ValidationRule
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return string.IsNullOrEmpty(_errorMessage) ? "You can't leave this field empty." : _errorMessage; }
            set { _errorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(value == null || value.ToString().Trim() != string.Empty, ErrorMessage);
        }
    }
}