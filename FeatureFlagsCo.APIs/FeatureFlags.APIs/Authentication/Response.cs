using System.Collections.Generic;

namespace FeatureFlags.APIs.Authentication
{
    public class Response
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<string> Messages { get; set; }
    }
}
