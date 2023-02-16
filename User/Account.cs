using SCCPP1.Session;

namespace SCCPP1.Entity
{
    public class Account
    {

        private readonly SessionData _sessionData;

        //the colleague's database ID
        public int ID { get; set; }

        //0 = admin, 1 = normal user
        public int Role { get; set; }

        //Name stored
        public string Name { get; set; }

        //Email stored (may not want to use signed on E-mail)
        public string Email { get; set; }

        public Account(SessionData sessionData)
        {
            this._sessionData = sessionData;
        }

    }
}
