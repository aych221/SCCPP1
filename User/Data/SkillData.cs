namespace SCCPP1.User.Data
{
    public class SkillData
    {

        private class SkillRecord
        {
            public int ID;

            public int Rating;

        }

        private List<SkillRecord> records;

        public SkillData()
        {
            records = new List<SkillRecord>();
        }
    }
}
