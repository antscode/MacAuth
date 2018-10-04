using System.Collections.Generic;

namespace MacAuth.ConfigModels
{
    public class MacAuthConfig
    {
        public string VerificationUrl { get; set; }
        public int ExpirySeconds { get; set; }
        public int MaxStoredRequests { get; set; }
        public IEnumerable<Provider> Providers { get; set; }
    }
}
