using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using SCCPP1.Models;

namespace SCCPP1.User.Data
{
    public class ProfileData : OwnerRecordData
    {


        private string title;
        public string Title
        {
            get { return title; }
            set { SetField(ref title, value); }
        }

        /// <summary>
        /// Used for loading ids from the database.
        /// </summary>
        private List<int> skillIDs, educationIDs, workIDs;

        protected List<int> SkillIDs
        {
            get { return skillIDs; }
            set { SetField(ref skillIDs, value); }
        }
        protected List<int> EducationIDs
        {
            get { return educationIDs; }
            set { SetField(ref educationIDs, value); }
        }
        protected List<int> WorkIDs
        {
            get { return workIDs; }
            set { SetField(ref workIDs, value); }
        }



        private Dictionary<int, SkillData> skills;

        protected Dictionary<int, SkillData> Skills
        {
            get { return skills; }
            set { SetField(ref skills, value); }
        }


        private Dictionary<int, EducationData> educationHistory;

        protected Dictionary<int, EducationData> EducationHistory
        {
            get { return educationHistory; }
            set { SetField(ref educationHistory, value); }
        }


        private Dictionary<int, WorkData> workHistory;

        protected Dictionary<int, WorkData> WorkHistory
        {
            get { return workHistory; }
            set { SetField(ref workHistory, value); }
        }

        /// <summary>
        /// Used to determine the ordering of the skill, education, and work sections.
        /// Skill is section 0, Education is section 1, Work is section 2.
        /// Future revisions could allow for more precise ordering down to the line.
        /// </summary>
        private string ordering;
        protected string Ordering
        {
            get { return ordering; }
            set { SetField(ref ordering, value); }
        }



        /*
         * 
         * 
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          title TEXT NOT NULL,          --similar to the file name
          education_history_ids TEXT,   --list of education history ids in specified order
          work_history_ids TEXT,        --list of work history ids in specified order
          colleague_skills_ids TEXT,    --list of skill ids in specified order
          ordering TEXT,                --The ordering of how the different sections will be (education, work, skills, etc)
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id)
         */

        public ProfileData(Account owner, int id = -1) : base(owner, id)
        {
            this.NeedsSave = false;
            this.IsUpdated = false;
        }


        //used when database loads profile
        public ProfileData(Account owner, int recordID, string title, List<int> skillIDs, List<int> educationIDs, List<int> workIDs, string ordering) : this(owner, recordID)
        {
            Title = title;
            SkillIDs = skillIDs;
            EducationIDs = educationIDs;
            WorkIDs = workIDs;
            Ordering = ordering;

            this.IsUpdated = true;
            this.NeedsSave = false;
        }


        //used when user creates profile that needs to be saved
        public ProfileData(Account owner, string title, List<int> skillIDs, List<int> educationIDs, List<int> workIDs, string ordering) :
            this(owner, -1, title, skillIDs, educationIDs, workIDs, ordering)
        {
            
            this.NeedsSave = true;
        }


        public Dictionary<int, SkillData> GetSkills()
        {

            if (!IsUpdated || skills == null)
                skills = new Dictionary<int, SkillData>();

            //cross-reference account skills with ids in this.
            //may need to save items before allowing profile to be edited, since it relies heavily on ids
            return skills;
        }


        public Dictionary<int, EducationData> GetEducationHistory()
        {

            if (!IsUpdated || skills == null)
                educationHistory = new Dictionary<int, EducationData>();

            //cross-reference account skills with ids in this.
            //may need to save items before allowing profile to be edited, since it relies heavily on ids
            return educationHistory;
        }


        public Dictionary<int, WorkData> GetWorkHistory()
        {

            if (!IsUpdated || skills == null)
                workHistory = new Dictionary<int, WorkData>();

            //cross-reference account skills with ids in this.
            //may need to save items before allowing profile to be edited, since it relies heavily on ids
            return workHistory;
        }


        public bool LoadData()
        {
            return IsUpdated = (Owner.LoadSkills(out skills) && Owner.LoadEducationHistory(out educationHistory) && Owner.LoadWorkHistory(out workHistory));
        }

        public override bool Save()
        {
            return false;
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        protected override bool Delete()
        {
            if (!Remove)
                return true;

            //TODO put database remove method
            //NeedsSave = !(IsUpdated
            return true;
        }
    }
}
