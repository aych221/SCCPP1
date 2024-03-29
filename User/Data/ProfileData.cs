﻿using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using SCCPP1.Database.Requests;
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
        /// Saved into and read from the database as a string array of true and false values.
        /// Pattern: ShowName|ShowEmailAddress|ShowPhoneNumber|ShowIntroNarrative
        /// </summary>
        #region About section
        private bool _showName;
        public bool ShowName
        {
            get { return _showName; }
            set { SetField(ref _showName, value); }
        }


        private bool _showPhoneNumber;
        public bool ShowPhoneNumber
        {
            get { return _showPhoneNumber; }
            set { SetField(ref _showPhoneNumber, value); }
        }


        private bool _showEmailAddress;
        public bool ShowEmailAddress
        {
            get { return _showEmailAddress; }
            set { SetField(ref _showEmailAddress, value); }
        }


        private bool _showIntroNarrative;
        public bool ShowIntroNarrative
        {
            get { return _showIntroNarrative; }
            set { SetField(ref _showIntroNarrative, value); }
        }
        #endregion


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


        //used when user creates profile that needs to be saved
        public ProfileData(Account owner, string title)
            : this(owner, -1, title, new(), new(), new(), new(), "")
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
                AddToHashSet(SelectedSkillIDs, skillRecordID);
        }


        /// <summary>
        /// Adds an EducationData object to the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be added.</param>
        public void AddEducation(int eduRecordID)
        {
            if (Owner.SavedEducationHistory.ContainsKey(eduRecordID))
                AddToHashSet(SelectedEducationIDs, eduRecordID);
        }


        /// <summary>
        /// Adds an CertificationData object to the profile using its RecordID.
        /// </summary>
        /// <param name="certRecordID">The CertificationData.RecordID to be added.</param>
        public void AddCertification(int certRecordID)
        {
            if (Owner.SavedCertifications.ContainsKey(certRecordID))
                AddToHashSet(SelectedCertificationIDs, certRecordID);
        }


        /// <summary>
        /// Adds a WorkData object to the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be added.</param>
        public void AddWork(int workRecordID)
        {
            if (Owner.SavedWorkHistory.ContainsKey(workRecordID))
                AddToHashSet(SelectedWorkIDs, workRecordID);
            //    workHistory.TryAdd(workRecordID, Owner.GetWorkData(workRecordID));
        }

        private void AddToHashSet(HashSet<int> hs, int id)
        {
            if (hs.Add(id))
                NeedsSave = true;
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
            RemoveFromHashSet(SelectedSkillIDs, skillRecordID);
        }


        /// <summary>
        /// Removes an EducationData object from the profile using its RecordID.
        /// </summary>
        /// <param name="eduRecordID">The EducationData.RecordID to be removed.</param>
        public void RemoveEducation(int eduRecordID)
        {
            RemoveFromHashSet(SelectedEducationIDs, eduRecordID);
        }


        /// <summary>
        /// Removes an CertificationData object from the profile using its RecordID.
        /// </summary>
        /// <param name="certRecordID">The CertificationData.RecordID to be removed.</param>
        public void RemoveCertification(int certRecordID)
        {
            //SelectedCertificationIDs.Remove(certRecordID);
            RemoveFromHashSet(SelectedCertificationIDs, certRecordID);
        }


        /// <summary>
        /// Removes a WorkData object from the profile using its RecordID.
        /// </summary>
        /// <param name="workRecordID">The WorkData.RecordID to be removed.</param>
        public void RemoveWork(int workRecordID)
        {
            //SelectedWorkIDs.Remove(workRecordID);
            RemoveFromHashSet(SelectedWorkIDs, workRecordID);
        }



        /// <summary>
        /// Removes an id from a hashset if it exists, and sets NeedsSave to true if it was removed.
        /// </summary>
        /// <param name="hs"></param>
        /// <param name="id"></param>
        private void RemoveFromHashSet(HashSet<int> hs, int id)
        {
            if (hs.Remove(id))
                NeedsSave = true;
        }
        #endregion


        public override bool Save()
        {
            if (!NeedsSave)
                return true;


            if (Program.DbRequestSystem)
                return !(NeedsSave = !(IsUpdated = DbRequestManager.Save(this)));
            else
            {
                //Console.WriteLine($"BeforeSave called on ProfileData (Remove={Remove}, Saved={IsUpdated})");
                IsUpdated = DatabaseConnector.SaveProfile(this);
                NeedsSave = false;
                //Console.WriteLine($"AfterSave called on ProfileData (Remove={Remove}, Saved={IsUpdated})");
                return IsUpdated && !NeedsSave;
            }
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            //Console.WriteLine($"BeforeDelete called on ProfileData (Remove={Remove}, Saved={IsRemoved})");
            Remove = NeedsSave = IsUpdated = true;
            //Console.WriteLine($"AfterDelete called on ProfileData (Remove={Remove}, Saved={IsRemoved})");
            return Remove;
        }
    }
}
