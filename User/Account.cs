using SCCPP1.Session;
using SCCPP1.User.Data;

namespace SCCPP1.User
{
    public class Account
    {

        private readonly SessionData Data;

        //the colleague's database ID
        public int RecordID { get; set; }

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



        public List<SkillData> Skills { get; set; }

        public List<EducationData> EducationHistory { get; set; }

        public List<WorkData> WorkHistory { get; set; }



        /// <summary>
        /// Are they a returning user?
        /// </summary>
        public readonly bool IsReturning;

        public Account(SessionData sessionData, bool isReturning)
        {
            this.Data = sessionData;
            this.IsReturning= isReturning;

            this.Email = Data.GetUsersEmail();
            this.Name = Data.GetUsersName();

        }

        public string GetUsername()
        {
            return Data.Username;
        }

    }
}
