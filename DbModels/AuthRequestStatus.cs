namespace MacAuth.DbModels
{
    public static class AuthRequestStatus
    {
        public static string Pending
        {
            get
            {
                return "pending";
            }
        }

        public static string Complete
        {
            get
            {
                return "complete";
            }
        }
    }
}
