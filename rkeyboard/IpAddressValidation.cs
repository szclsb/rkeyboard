using System;
using System.Globalization;
using System.Net;
using System.Windows.Controls;

namespace rkeyboard {
    public class IpAddressValidation : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var str = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(str)) {
                return new ValidationResult(false, "Invalid input");
            }

            return IPAddress.TryParse(str, out var address)
                ? new ValidationResult(true, null)
                : new ValidationResult(false, "Invalid ip address");
        }
    }
}