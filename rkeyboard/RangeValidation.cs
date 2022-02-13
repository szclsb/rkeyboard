using System;
using System.Globalization;
using System.Windows.Controls;

namespace rkeyboard
{
    public class RangeValidation : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "Invalid input");
            }

            if (int.TryParse(str, out var port))
            {
                return port >= Min && port <= Max
                    ? new ValidationResult(true, null)
                    : new ValidationResult(false, $"Input is not between [${Min}, ${Max}]");
            }

            return new ValidationResult(false, "input is not an integer");
        }
    }
}