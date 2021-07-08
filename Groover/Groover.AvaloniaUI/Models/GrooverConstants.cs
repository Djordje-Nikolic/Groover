using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class GrooverConstants
    {
        public TimeSpan LockoutLength { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int PasswordMinLength { get; set; }
        public int PasswordMinUnique { get; set; }
        public bool PasswordRequireDigit { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }
        
        public GrooverConstants(NameValueCollection nvC)
        {
            if (nvC == null)
                throw new ArgumentNullException();

            LockoutLength = TimeSpan.Parse(nvC["LockoutLength"] ?? "00:00:00");
            MaxLoginAttempts = int.Parse(nvC["MaxLoginAttempts"] ?? "0");
            PasswordMinLength = int.Parse(nvC["PasswordMinLength"] ?? "0");
            PasswordMinUnique = int.Parse(nvC["PasswordMinUnique"] ?? "0");
            PasswordRequireDigit = bool.Parse(nvC["PasswordRequireDigit"] ?? "false");
            PasswordRequireLowercase = bool.Parse(nvC["PasswordRequireLowercase"] ?? "false");
            PasswordRequireUppercase = bool.Parse(nvC["PasswordRequireUppercase"] ?? "false");
            PasswordRequireNonAlphanumeric = bool.Parse(nvC["PasswordRequireNonAlphanumeric"] ?? "false");
        }
    }
}
