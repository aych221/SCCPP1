using SCCPP1.Database.Entity;
using SCCPP1.Database.Requests;

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

        private string skillCategoryName;
        public string SkillCategoryName
        {
            get { return skillCategoryName; }
            set { SetField(ref skillCategoryName, value); }
        }

        public int SkillCategoryID { get; set; }


        public SkillData(Account owner, int recordID = -1) : base(owner, recordID)
        {
            Rating = -1;
        }

        public SkillData(Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating) : this(owner, recordID)
        {
            this.SkillCategoryName = skillCategoryName;
            this.SkillCategoryID = skillCategoryID;
            this.SkillName = skillName;
            this.SkillID = skillID;
            this.Rating = rating;

            this.IsUpdated = true;
        }

        //load from database
        public SkillData(Account owner, int recordID, int skillCategoryID, int skillID, int rating) :
            this(owner, recordID, null, skillCategoryID, null, skillID, rating)
        {
            this.NeedsSave = false;
        }

        //load from database
        public SkillData(Account owner, string skillCategoryName, string skillName, int rating) :
            this(owner, -1, skillCategoryName, -1, skillName, -1, rating)
        {
            this.NeedsSave = true;
        }


        //load from database (no skill category)
        public SkillData(Account owner, int recordID, int skillID, int rating) :
            this(owner, recordID, null, -1, null, skillID, rating)
        {
            this.NeedsSave = false;
        }

        //load from database (no skill category
        public SkillData(Account owner, string skillName, int rating) :
            this(owner, -1, null, -1, skillName, -1, rating)
        {
            this.NeedsSave = true;
        }



        public override bool Save()
        {
            //save resources if we don't need to save.
            /*if (!NeedsSave)
                return true;*/

            if (Program.DbRequestSystem)
                return !(NeedsSave = !(IsUpdated = DbRequestManager.Save(this)));
            else
            {
                NeedsSave = !(IsUpdated = DatabaseConnector.SaveColleagueSkill(this));
                return IsUpdated && !NeedsSave;
            }
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            return Remove = NeedsSave = IsUpdated = true;
        }


        public DbColleagueSkillsRecord ToDbRecord()
        {
            return new DbColleagueSkillsRecord(this);
        }
    }
}
