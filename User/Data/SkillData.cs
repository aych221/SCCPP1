namespace SCCPP1.User.Data
{
    public class SkillData : AccountRecordData
    {

        public string Skill { get; set; }

        public int Rating { get; set; }

        public SkillData(Account owner, int id = -1) : base(owner, id)
        {

        }
    }
}
