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

        public int Phone { get; set; }

        public string Address { get; set; }

        public string IntroNarrative { get; set; }

        public int MainProfileID { get; set; }


        /// <summary>
        /// Are they a returning user?
        /// </summary>
        public readonly bool IsReturning;

        public Account(SessionData sessionData, bool isReturning)
        {
            this._sessionData = sessionData;
            this.IsReturning= isReturning;
        }

        public string GetUsername()
        {
            return _sessionData.Username;
        }

    }
}
