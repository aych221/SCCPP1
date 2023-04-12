using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using SCCPP1.Models;

namespace SCCPP1.User.Data
{
    public class ProfileData : OwnerRecordData
    {


        private string title;

        /// <summary>
        /// The title of this profile, does not need to be unique.
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetField(ref title, value == null ? "" : value); }
        }


        /// <summary>
        /// Used to hold RecordData.RecordIDs that are selected (by the user) to appear on this profile.
        /// </summary>
        private HashSet<int> _skillIDs, _educationIDs, _certificationIDs, _workIDs;


        /// <summary>
        /// The set of SkillData.RecordIDs that are selected (by the user) to appear on this profile.
        /// </summary>
        public HashSet<int> SelectedSkillIDs
        {
            get { return _skillIDs; }
            protected set { SetField(ref _skillIDs, value); }
        }

        /// <summary>
        /// The set of EducationData.RecordIDs that are selected (by the user) to appear on this profile.
        /// </summary>
        public HashSet<int> SelectedEducationIDs
        {
            get { return _educationIDs; }
            protected set { SetField(ref _educationIDs, value); }
        }

        /// <summary>
        /// The set of CertificationData.RecordIDs that are selected (by the user) to appear on this profile.
        /// </summary>
        public HashSet<int> SelectedCertificationIDs
        {
            get { return _certificationIDs; }
            protected set { SetField(ref _certificationIDs, value); }
        }


        /// <summary>
        /// The set of WorkData.RecordIDs that are selected (by the user) to appear on this profile.
        /// </summary>
        public HashSet<int> SelectedWorkIDs
        {
            get { return _workIDs; }
            protected set { SetField(ref _workIDs, value); }
        }


        /// <summary>
        /// Used to determine the ordering of the skill, education, and work sections.
        /// Skill is section 0, Education is section 1, Work is section 2.
        /// Future revisions could allow for more precise ordering down to the line.
        /// </summary>
        private string ordering;


        /// <summary>
        /// Used to determine the ordering of the skill, education, and work sections.
        /// Skill is section 0, Education is section 1, Work is section 2.
        /// Future revisions could allow for more precise ordering down to the line.
        /// </summary>
        public string Ordering
        {
            get { return ordering; }
            protected set { SetField(ref ordering, value); }
        }




        public ProfileData(Account owner, int id = -1) : base(owner, id)
        {
            this.NeedsSave = false;
            this.IsUpdated = false;
        }




        //used when database loads profile
        public ProfileData(Account owner, int recordID, string title, HashSet<int> skillIDs, HashSet<int> educationIDs, HashSet<int> certificationIDs, HashSet<int> workIDs, string ordering) :
            this(owner, recordID)
        {
            Title = title;
            SelectedSkillIDs = skillIDs;
            SelectedEducationIDs = educationIDs;
            SelectedCertificationIDs = certificationIDs;
            SelectedWorkIDs = workIDs;
            Ordering = ordering;

            this.IsUpdated = true;
            this.NeedsSave = false;
        }


        //used when user creates profile that needs to be saved
        public ProfileData(Account owner, string title, HashSet<int> skillIDs, HashSet<int> educationIDs, HashSet<int> certificationIDs, HashSet<int> workIDs, string ordering) :
            this(owner, -1, title, skillIDs, educationIDs, certificationIDs, workIDs, ordering)
        {
            
            this.NeedsSave = true;
        }


        //Content add methods
        #region Add Methods

        /// <summary>
        /// Adds a SkillData object to the profile using its RecordID.
        /// </summary>
        /// <param name="skillRecordID">The SkillData.RecordID to be added.</param>
        public void AddSkill(int skillRecordID)
        {
            if (Owner.SavedSkills.ContainsKey(skillRecordID))
                SelectedSkillIDs.Add(skillRecordID);
        }


        /// <summary>
        /// Adds an EducationData object to the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be added.</param>
        public void AddEducation(int eduRecordID)
        {
            if (Owner.SavedEducationHistory.ContainsKey(eduRecordID))
                SelectedEducationIDs.Add(eduRecordID);
        }


        /// <summary>
        /// Adds an CertificationData object to the profile using its RecordID.
        /// </summary>
        /// <param name="certRecordID">The CertificationData.RecordID to be added.</param>
        public void AddCertification(int certRecordID)
        {
            if (Owner.SavedCertifications.ContainsKey(certRecordID))
                SelectedCertificationIDs.Add(certRecordID);
        }


        /// <summary>
        /// Adds a WorkData object to the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be added.</param>
        public void AddWork(int workRecordID)
        {
            if (Owner.SavedWorkHistory.ContainsKey(workRecordID))
                SelectedWorkIDs.Add(workRecordID);
            //    workHistory.TryAdd(workRecordID, Owner.GetWorkData(workRecordID));
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
            SelectedSkillIDs.Remove(skillRecordID);
        }


        /// <summary>
        /// Removes an EducationData object from the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be removed.</param>
        public void RemoveEducation(int eduRecordID)
        {
            SelectedEducationIDs.Remove(eduRecordID);
        }


        /// <summary>
        /// Removes an CertificationData object from the profile using its RecordID.
        /// </summary>
        /// <param name="certRecordID">The CertificationData.RecordID to be removed.</param>
        public void RemoveCertification(int certRecordID)
        {
            SelectedCertificationIDs.Remove(certRecordID);
        }


        /// <summary>
        /// Removes a WorkData object from the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be removed.</param>
        public void RemoveWork(int workRecordID)
        {
            SelectedWorkIDs.Remove(workRecordID);
        }

        #endregion

        public override bool Save()
        {
            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveProfile(this));
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            if (!Remove)
                return true;

            //TODO put database remove method
            //NeedsSave = !(IsUpdated
            return true;
        }
    }
}
