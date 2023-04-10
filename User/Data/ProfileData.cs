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
        private HashSet<int> skillIDs, educationIDs, workIDs;


        protected HashSet<int> SkillIDs
        {
            get { return skillIDs; }
            set { SetField(ref skillIDs, value); }
        }
        protected HashSet<int> EducationIDs
        {
            get { return educationIDs; }
            set { SetField(ref educationIDs, value); }
        }
        protected HashSet<int> WorkIDs
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


        public ProfileData(Account owner, int id = -1) : base(owner, id)
        {
            this.NeedsSave = false;
            this.IsUpdated = false;
        }


        //used when database loads profile
        public ProfileData(Account owner, int recordID, string title, HashSet<int> skillIDs, HashSet<int> educationIDs, HashSet<int> workIDs, string ordering) :
            this(owner, recordID)
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
        public ProfileData(Account owner, string title, HashSet<int> skillIDs, HashSet<int> educationIDs, HashSet<int> workIDs, string ordering) :
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



        //Content add methods
        #region Add Methods

        /// <summary>
        /// Adds a SkillData object to the profile using its RecordID.
        /// </summary>
        /// <param name="skillRecordID">The SkillData.RecordID to be added.</param>
        public void AddSkill(int skillRecordID)
        {
            if (skillIDs.Add(skillRecordID))
                skills.TryAdd(skillRecordID, Owner.GetSkillData(skillRecordID));
        }


        /// <summary>
        /// Adds an EducationData object to the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be added.</param>
        public void AddEducation(int eduRecordID)
        {
            if (educationIDs.Add(eduRecordID))
                educationHistory.TryAdd(eduRecordID, Owner.GetEducationData(eduRecordID));
        }


        /// <summary>
        /// Adds a WorkData object to the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be added.</param>
        public void AddWork(int workRecordID)
        {
            if (workIDs.Add(workRecordID))
                workHistory.TryAdd(workRecordID, Owner.GetWorkData(workRecordID));
        }

        #endregion



        //Content remove methods
        #region Remove Methods

        /// <summary>
        /// Removes a SkillData object from the profile using its RecordID.
        /// </summary>
        /// <param name="skillRecordID">The SkillData.RecordID to be removed.</param>
        public void RemoveSkill(int skillRecordID)
        {
            if (skillIDs.Remove(skillRecordID))
                skills.Remove(skillRecordID);
        }


        /// <summary>
        /// Removes an EducationData object from the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be removed.</param>
        public void RemoveEducation(int eduRecordID)
        {
            if (educationIDs.Remove(eduRecordID))
                educationHistory.Remove(eduRecordID);
        }


        /// <summary>
        /// Removes a WorkData object from the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be removed.</param>
        public void RemoveWork(int workRecordID)
        {
            if (workIDs.Remove(workRecordID))
                workHistory.Remove(workRecordID);
        }

        #endregion




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
