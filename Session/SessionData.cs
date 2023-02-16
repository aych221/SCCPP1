using SCCPP1.Entity;

namespace SCCPP1.Session
{
    public class SessionData
    {
        //This is the session ID, not necessarily the Colleages ID
        public int ID { get; set; }
        public string Username { get; set; }
        public bool SignedOn { get; set; }

        public Account Account  { get; set; }

        public SessionData(string username)
        { 
            this.SignedOn = true;
            this.Username = username;
        }

    }
}
