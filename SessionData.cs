namespace SCCPP1
{
    public class SessionData
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public bool SignedOn { get; set; }

        public SessionData(string username)
        { 
            this.SignedOn = true;
            this.Username = username;
        }

    }
}
