using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Infrastructure.Options
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
