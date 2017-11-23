using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Views.Validations
{
    public class IsNonExistentConnection : ValidationRule
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return string.IsNullOrEmpty(_errorMessage) ? "This connection name already exists." : _errorMessage; }
            set { _errorMessage = value; }
        }

        public string OriginalValue { get; set; }
        public ItemState ItemState { get; set; }
        public List<string> ConnectionNames { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var v = value as string;
            var result = v == OriginalValue || !ConnectionNames.Contains(v);
            return new ValidationResult(result, ErrorMessage);
        }
    }
}