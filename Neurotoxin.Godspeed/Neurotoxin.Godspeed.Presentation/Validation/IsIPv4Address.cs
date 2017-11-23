using System;
using System.Globalization;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Validation
{
    public class IsIPv4Address : ValidationRule
    {
        private string _emptyErrorMessage;
        public string EmptyErrorMessage
        {
            get { return string.IsNullOrEmpty(_emptyErrorMessage) ? "Please enter an IP Address." : _emptyErrorMessage; }
            set { _emptyErrorMessage = value; }
        }

        private string _wrongFormatErrorMessage;
        public string WrongFormatErrorMessage
        {
            get { return string.IsNullOrEmpty(_wrongFormatErrorMessage) ? "IP Address should be four octets, seperated by decimals." : _wrongFormatErrorMessage; }
            set { _wrongFormatErrorMessage = value; }
        }

        private string _invalidCharacterErrorMessage;
        public string InvalidCharacterErrorMessage
        {
            get { return string.IsNullOrEmpty(_invalidCharacterErrorMessage) ? "Each octet of an IP Address should be a number." : _invalidCharacterErrorMessage; }
            set { _invalidCharacterErrorMessage = value; }
        }

        private string _octetOutOfRangeErrorMessage;
        public string OctetOutOfRangeErrorMessage
        {
            get { return string.IsNullOrEmpty(_octetOutOfRangeErrorMessage) ? "Each octet of an IP Address should be between 0 and 255." : _octetOutOfRangeErrorMessage; }
            set { _octetOutOfRangeErrorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == string.Empty)
            {
                return new ValidationResult(false, EmptyErrorMessage);
            }

            if (str != null)
            {
                var parts = str.Split('.');
                if (parts.Length != 4)
                {
                    return new ValidationResult(false, WrongFormatErrorMessage);
                }

                foreach (var p in parts)
                {
                    int intPart;
                    if (!int.TryParse(p, NumberStyles.Integer, cultureInfo.NumberFormat, out intPart))
                    {
                        return new ValidationResult(false, InvalidCharacterErrorMessage);
                    }

                    if (intPart < 0 || intPart > 255)
                    {
                        return new ValidationResult(false, OctetOutOfRangeErrorMessage);
                    }
                }
            }

            return new ValidationResult(true, null);
        }
    }
}