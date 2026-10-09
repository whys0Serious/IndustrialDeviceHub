using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Models
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "DeviceHub";
        public string Audience { get; set; } = "DeviceHub";
        public int ExpireHours { get; set; } = 24;
    }
}
