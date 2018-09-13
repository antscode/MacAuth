namespace MacAuth.Models
{
    public class CodeResponse
    {
        public string device_code { get; set; }
        public string user_code { get; set; }
        public string verification_url { get; set; }
        public int expires_in { get; set; }
        public string error { get; set; }
    }
}
