namespace SCCPP1.User.Data
{
    public class SkillData : OwnerRecordData
    {

        private string skillName;
        public string SkillName
        {
            get { return skillName; }
            set { SetField(ref skillName, value); }
        }

        public int SkillID { get; set; }


        private int rating;
        public int Rating
        {
            get { return rating; }
            set { SetField(ref rating, value); }
        }


        public SkillData(Account owner, int recordID = -1) : base(owner, recordID)
        {
            Rating = -1;
        }

        public SkillData(Account owner, int recordID, string skillName, int skillID, int rating) : this(owner, recordID)
        {
            this.SkillName = skillName;
            this.SkillID = skillID;
            this.Rating = rating;

            this.IsUpdated = true;
        }


        //input from user
        public SkillData(Account owner, string skillName, int rating) : this(owner, -1, skillName, -1, rating)
        {
            this.NeedsSave = true;
        }


        //loaded from db
        public SkillData(Account owner, int recordID, int skillID, int rating) : this(owner, recordID, null, skillID, rating)
        {
            this.NeedsSave = false;
            //maybe use IsUpdated = false flag to let DB know we need null values filled?
        }


        public override bool Save()
        {
            //save resources if we don't need to save.
            if (!NeedsSave)
                return true;

            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveColleageSkills(this));
        }
    }
}
