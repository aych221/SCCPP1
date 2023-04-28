using SCCPP1.Database;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Requests;
using SCCPP1.Database.Sqlite;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User.Data;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.Intrinsics.X86;

namespace SCCPP1.User
{
    public class Account : RecordData
    {

        public readonly SessionData Data;


        private ProfileData chosenProfile;
        public void ChooseProfile(ProfileData pd)
        {
            chosenProfile = pd;
        }
        public ProfileData ChosenProfile()
        {
            return chosenProfile;
        }


        #region Account Data properties
        //TODO: change all properties to use protected sets
        //0 = admin, 1 = normal user
        private int _role;
        public int Role
        {
            get { return _role; }
            set { SetField(ref _role, value); }
        }


        //Name stored
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set { SetField(ref _firstName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set { SetField(ref _lastName, value); }
        }


        private string? _middleName;
        public string? MiddleName
        {
            get { return _middleName; }
            set { SetField(ref _middleName, value); }
        }


        //Email stored (may not want to use signed on E-mail)
        private string _emailAddress;
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { SetField(ref _emailAddress, value); }
        }

        private long _phoneNumber;
        public long PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetField(ref _phoneNumber, value); }
        }


        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set { SetField(ref _streetAddress, value); }
        }


        private Location _location;
        public Location Location
        {
            get { return _location; }
            set { SetField(ref _location, value); }
        }


        private string _introNarrative;
        public string IntroNarrative
        {
            get { return _introNarrative; }
            set { SetField(ref _introNarrative, value); }
        }


        private int _mainProfileID;
        public int MainProfileID
        {
            get { return _mainProfileID; }
            set { SetField(ref _mainProfileID, value); }
        }

        #endregion



        //may want to change these to dictionaries
        [Obsolete("Use SavedSkills instead", false)]
        public List<SkillData> Skills { get; set; }


        [Obsolete("Use SavedEducation instead", false)]
        public List<EducationData> EducationHistory { get; set; }


        [Obsolete("Use SavedWorkHistory instead", false)]
        public List<WorkData> WorkHistory { get; set; }


        [Obsolete("Use SavedProfiles instead", false)]
        public List<ProfileData> Profiles { get; set; }



        #region Unsaved Data Lists
        private List<SkillData> _unsavedSkills;

        protected List<SkillData> UnsavedSkills
        {
            get { return _unsavedSkills; }
            set { SetField(ref _unsavedSkills, value); }
        }



        private List<EducationData> _unsavedEducationHistory;

        protected List<EducationData> UnsavedEducationHistory
        {
            get { return _unsavedEducationHistory; }
            set { SetField(ref _unsavedEducationHistory, value); }
        }



        private List<CertificationData> _unsavedCertificates;

        protected List<CertificationData> UnsavedCertifications
        {
            get { return _unsavedCertificates; }
            set { SetField(ref _unsavedCertificates, value); }
        }



        private List<WorkData> _unsavedWorkHistory;

        protected List<WorkData> UnsavedWorkHistory
        {
            get { return _unsavedWorkHistory; }
            set { SetField(ref _unsavedWorkHistory, value); }
        }



        private List<ProfileData> _unsavedProfiles;
        
        protected List<ProfileData> UnsavedProfiles
        {
            get { return _unsavedProfiles; }
            set { SetField(ref _unsavedProfiles, value); }
        }
        #endregion


        #region Saved Data Dictionaries
        private ReadOnlyDictionary<int, SkillData> _savedSkills;

        public ReadOnlyDictionary<int, SkillData> SavedSkills
        {
            get { return _savedSkills; }
            protected set { SetField(ref _savedSkills, value); }
        }



        private ReadOnlyDictionary<int, EducationData> _savedEducationHistory;

        public ReadOnlyDictionary<int, EducationData> SavedEducationHistory
        {
            get { return _savedEducationHistory; }
            protected set { SetField(ref _savedEducationHistory, value); }
        }



        private ReadOnlyDictionary<int, CertificationData> _savedCertifications;

        public ReadOnlyDictionary<int, CertificationData> SavedCertifications
        {
            get { return _savedCertifications; }
            protected set { SetField(ref _savedCertifications, value); }
        }



        private ReadOnlyDictionary<int, WorkData> _savedWorkHistory;

        public ReadOnlyDictionary<int, WorkData> SavedWorkHistory
        {
            get { return _savedWorkHistory; }
            protected set { SetField(ref _savedWorkHistory, value); }
        }


        private ReadOnlyDictionary<int, ProfileData> _savedProfiles;

        public ReadOnlyDictionary<int, ProfileData> SavedProfiles
        {
            get { return _savedProfiles; }
            protected set { SetField(ref _savedProfiles, value); }
        }
        #endregion


        #region Delete Data Lists
        private List<SkillData> _deletedSkills;

        protected List<SkillData> DeletedSkills { get; set; }

        private List<EducationData> _deletedEducation;

        protected List<EducationData> DeletedEducation { get; set; }


        private List<CertificationData> _deletedCertifications;

        protected List<CertificationData> DeletedCertifications { get; set; }


        private List<WorkData> _deletedWork;

        protected List<WorkData> DeletedWork { get; set; }


        private List<ProfileData> _deletedProfiles;

        protected List<ProfileData> DeletedProfiles { get; set; }

        #endregion




        /// <summary>
        /// Are they a returning user?
        /// </summary>
        public bool IsReturning { get; set; }



        public Account(SessionData sessionData, bool isReturning) : base()
        {
            this.Data = sessionData;
            this.IsReturning = isReturning;

            this.EmailAddress = Data.GetUsersEmail();
            this.Name = Data.GetUsersName();

            UnsavedSkills = new List<SkillData>();
            UnsavedEducationHistory = new List<EducationData>();
            UnsavedCertifications = new List<CertificationData>();
            UnsavedWorkHistory = new List<WorkData>();
            UnsavedProfiles = new List<ProfileData>();

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(new Dictionary<int, SkillData>());
            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(new Dictionary<int, CertificationData>());
            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(new Dictionary<int, EducationData>());
            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(new Dictionary<int, WorkData>());
            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(new Dictionary<int, ProfileData>());
        }

#if DEBUG
        /// <summary>
        /// This constructor should only be used for testing purposes. This is not valid for a real account.
        /// </summary>
        /// <param name="username"></param>
        public Account(string username)
        {
            this.Data = null;
            this.IsReturning = false;

            this.Name = username;

            UnsavedSkills = new List<SkillData>();
            UnsavedEducationHistory = new List<EducationData>();
            UnsavedCertifications = new List<CertificationData>();
            UnsavedWorkHistory = new List<WorkData>();
            UnsavedProfiles = new List<ProfileData>();

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(new Dictionary<int, SkillData>());
            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(new Dictionary<int, CertificationData>());
            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(new Dictionary<int, EducationData>());
            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(new Dictionary<int, WorkData>());
            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(new Dictionary<int, ProfileData>());

            //may not need these
            DeletedSkills = new List<SkillData>();
            DeletedEducation = new List<EducationData>();
            DeletedCertifications = new List<CertificationData>();
            DeletedWork = new List<WorkData>();
            DeletedProfiles = new List<ProfileData>();

        }
#endif

        #region Add/Remove/Edit data methods

        /// <summary>
        /// Updates the basic account data for this colleague. Updates NeedsSave and IsUpdated flags.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <param name="emailAddress"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="introNarrative"></param>
        public void UpdateData(string firstName, string middleName, string lastName, string emailAddress, long phoneNumber, string introNarrative)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Name = Utilities.ToFullName(firstName, middleName, lastName);
            EmailAddress = emailAddress;
            PhoneNumber = phoneNumber;
            IntroNarrative = introNarrative;
            NeedsSave = IsUpdated = true;
        }


        /// <summary>
        /// Updates the basic account data for this colleague. Updates NeedsSave and IsUpdated flags.
        /// </summary>
        /// <param name="name">Passed in as "LastName, FirstName [MiddleName]"</param>
        /// <param name="emailAddress"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="introNarrative"></param>
        public void UpdateData(string name, string emailAddress, long phoneNumber, string introNarrative)
        {
            Name = name;
            string[] names = Utilities.SplitFullName(Name);
            UpdateData(names[1], names[2], names[0], emailAddress, phoneNumber, introNarrative);
        }


        /// <summary>
        /// Adds a list of skills to the account.
        /// </summary>
        /// <param name="skillNames"></param>
        public void AddSkills(params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            foreach (string skillName in skillNames)
            {
                //Skills.Add(new SkillData(this, skillName, -1));
                UnsavedSkills.Add(new SkillData(this, skillName, -1));
            }

            NeedsSave = IsUpdated = true;
        }


        /// <summary>
        /// Adds a list of skills to the account for the specified skill category.
        /// </summary>
        /// <param name="skillCategory">Category of skill</param>
        /// <param name="skillNames"></param>
        public void AddSkills(string skillCategory, params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            //first make a dictionary of all the skills in the category
            //(used to prevent duplicates from being added)
            Dictionary<string, SkillData> skillsInCategory = SavedSkills.Values
                .ToList()
                .Concat(UnsavedSkills)
                .Where(s => s.SkillCategoryName.Equals(skillCategory))
                .ToDictionary(s => s.SkillName, s => s);

            foreach (string skillName in skillNames)
            {
                //don't add duplicates
                if (skillsInCategory.ContainsKey(skillName))
                    continue;

                UnsavedSkills.Add(new SkillData(this, skillCategory, skillName, -1));
            }

            NeedsSave = IsUpdated = true;
        }


        public void AddEducation(string institution, string degreeType, string field, Location? location, DateOnly? startDate, DateOnly? endDate)
        {
            //EducationHistory.Add(new EducationData(this, institution, degreeType, field, new Location(), new DateOnly(), new DateOnly()));
            UnsavedEducationHistory.Add(new EducationData(this, institution, degreeType, field, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        public void AddEducation(string institution, string degreeType, string field)
        {
            AddEducation(institution, degreeType, field, null, null, null);
        }



        public void AddCertification(string institution, string certificationType, string description, Location? location, DateOnly? startDate, DateOnly? endDate)
        {

            UnsavedCertifications.Add(new CertificationData(this, institution, certificationType, description, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        public void AddCertification(string institution, string certificationType, DateOnly? startDate, DateOnly? endDate)
        {
            AddCertification(institution, certificationType, null, null, startDate.GetValueOrDefault(), endDate.GetValueOrDefault());
        }


        public void AddWork(string employer, string jobTitle, string description, Location? location, DateOnly? startDate, DateOnly? endDate)
        {
            //WorkHistory.Add(new WorkData(this, employer, jobTitle, description, location, startDate, endDate));
            UnsavedWorkHistory.Add(new WorkData(this, employer, jobTitle, description, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        #region Remove Data methods
        public void RemoveSkill(int id)
        {
            if ((GetSkillData(id)?.Delete()) is null)
                UnsavedSkills.Find(x => x.RecordID == id)?.Delete();
        }

        public void RemoveSkills(params int[] ids)
        {
            foreach (int id in ids)
            {
                //delete all in the dictionary
                RemoveSkill(id);
            }
        }

        public void RemoveEducation(int id)
        {
            //delete all in the dictionary
            if ((GetEducationData(id)?.Delete()) is null)
                UnsavedEducationHistory.Find(x => x.RecordID == id)?.Delete();
        }

        public void RemoveEducations(params int[] ids)
        {
            foreach (int id in ids)
            {
                //delete all in the dictionary
                RemoveEducation(id);
            }
        }

        public void RemoveCertification(int id)
        {
            //delete all in the dictionary
            if ((GetCertificationData(id)?.Delete()) is null)
                UnsavedCertifications.Find(x => x.RecordID == id)?.Delete();
        }

        public void RemoveCertifications(params int[] ids)
        {
            foreach (int id in ids)
            {
                //delete all in the dictionary
                RemoveCertification(id);
            }
        }

        public void RemoveWork(int id)
        {
            //delete all in the dictionary
            if ((GetWorkData(id)?.Delete()) is null)
                UnsavedWorkHistory.Find(x => x.RecordID == id)?.Delete();
        }

        public void RemoveWorks(params int[] ids)
        {
            foreach (int id in ids)
            {
                //delete all in the dictionary
                if ((GetWorkData(id)?.Delete()) is null)
                    UnsavedWorkHistory.Find(x => x.RecordID == id)?.Delete();
            }
        }



        public void RemoveProfile(int id)
        {
            bool? success = false;
            Console.WriteLine("Removing profile with id: " + id);
            //delete all in the dictionary
            if ((success = (GetProfileData(id)?.Delete())) is null)
                success = (UnsavedProfiles.Find(x => x.RecordID == id)?.Delete());
            if ((success is not null) && success.Value)
                Console.WriteLine("Profile " + id + " has remove flag set");
        }



        public void RemoveProfiles(params int[] ids)
        {
            foreach (int id in ids)
            {
                bool? success = false; ;
                Console.WriteLine("Removing profile with id: " + id);
                //delete all in the dictionary
                if ((success = (GetProfileData(id)?.Delete())) is null)
                    success = (UnsavedProfiles.Find(x => x.RecordID == id)?.Delete());
                if ((success is not null) && success.Value)
                    Console.WriteLine("Profile " + id + " has remove flag set");
            }
        }

        #endregion

        //used to update account's data without deleting and creating new objects
        #region Edit Data methods
        public void EditSkills(string category, params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            //search through entire list of skills
            foreach (SkillData d in SavedSkills.Values.ToList().Concat(UnsavedSkills))
            {
             //   if (d.SkillCategoryName.Equals(category))
            }

            NeedsSave = IsUpdated = true;
        }
        #endregion
        #endregion




        #region Get Data methods

        /// <summary>
        /// Gets the username of the account.
        /// This method is used in <see cref="Account.Equals(object)"/>.
        /// It is important that it always returns a unique value in the scope of runtime.
        /// If the value entered is null, the property will assign the value returned from
        /// <see cref="Utilities.ToSHA256Hash(string)"/> where the string is set to $"AccountHashCode={<see cref="object.GetHashCode()"/>}".
        /// </summary>
        /// <returns>The SessionData.Username</returns>
        public string GetUsername()
        {
            //Data will only be null if the account is not logged in. (mainly for testing)
            if (Data != null)
                return Data.Username;
            else if (Name != null)
                return Name;
            else
                return Utilities.ToSHA256Hash($"AccountHashCode={GetHashCode()}");
        }


        /// <summary>
        /// Attempts to fetch a saved SkillData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>SkillData if object is found, null otherwise</returns>
        public SkillData? GetSkillData(int id)
        {
            if (SavedSkills.TryGetValue(id, out SkillData data))
                return data;
            return null;
        }


        /// <summary>
        /// Attempts to fetch a saved EducationData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>EducationData if object is found, null otherwise</returns>
        public EducationData? GetEducationData(int id)
        {
            if (SavedEducationHistory.TryGetValue(id, out EducationData data))
                return data;
            return null;
        }


        /// <summary>
        /// Attempts to fetch a saved WorkData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>WorkData if object is found, null otherwise</returns>
        public WorkData? GetWorkData(int id)
        {
            if (SavedWorkHistory.TryGetValue(id, out WorkData data))
                return data;
            return null;
        }


        /// <summary>
        /// Attempts to fetch a saved ProfileData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ProfuleData if object is found, null otherwise</returns>
        public ProfileData? GetProfileData(int id)
        {
            if (SavedProfiles.TryGetValue(id, out ProfileData data))
                return data;
            return null;
        }


        /// <summary>
        /// Attempts to fetch a saved CertificationData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CertificationData if object is found, null otherwise</returns>
        public CertificationData? GetCertificationData(int id)
    {
            if (SavedCertifications.TryGetValue(id, out CertificationData data))
                return data;
            return null;
        }


        #endregion




        #region Profile methods

        /// <summary>
        /// Creates a new profile object. It is important to call <see cref="PersistAll"/> before creating a new profile or multiple profiles.
        /// A profile can only use data that has been saved to the database.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>A new profile object for the user</returns>
        public ProfileData CreateProfile(string title)
        {
            //might want to load main profile as default.



            //may not need in the future, considered an expensive call.
            //PersistAll();

            //might want to create these hashsets in LoadAll() method to reduce memory and computations
            ProfileData pd = new ProfileData(this, title);

            //new HashSet<int>(SavedSkills.Keys),
            //new HashSet<int>(SavedEducationHistory.Keys),
            //new HashSet<int>(SavedCertifications.Keys),
            //new HashSet<int>(SavedWorkHistory.Keys),
            //""
            //);

            pd.ShowName = pd.ShowEmailAddress = pd.ShowPhoneNumber = pd.ShowIntroNarrative = true;
            UnsavedProfiles.Add(pd);
            return pd;
        }

        #endregion




        #region Save methods

        /// <summary>
        /// Saves the user's direct profile information, does not save associated data.
        /// </summary>
        /// <returns>true if changes are saved in database, false otherwise.</returns>
        public override bool Save()
        {
            /* if (!NeedsSave)
                 return true;*/
            if (Program.DbRequestSystem)
                return !(NeedsSave = !(IsUpdated = DbRequestManager.Save(this)));
            else
                return !(NeedsSave = !(IsUpdated = DatabaseConnector.SaveUser(this)));

        }

#endregion



#region Delete methods
        /// <summary>
        /// Deletes the user's data and all records associated with it.
        /// </summary>
        /// <returns>true if records were removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            return Remove = NeedsSave = IsUpdated = true;
        }

#endregion


#region Persist methods
        /// <summary>
        /// This method will save, update, or delete and reload data to ensure all data is updated in the database.
        /// </summary>
        /// <returns>true if the operation was successful, false otherwise.</returns>
        public bool PersistAll()
        {
            Console.WriteLine("**********************Persisting all data for user..");
            bool success1 = Persist(),
                success2 = PersistSkills(),
                success3 = PersistCertifications(),
                success4 = PersistEducationHistory(),
                success5 = PersistWorkHistory(),
                success6 = PersistProfiles();
            bool success = success1 && success2 && success3 && success4 && success5 && success6;
#if DEBUG
            Console.WriteLine("**********************All data persisted? " + success);
            if (!success)
            {
                Console.WriteLine("**********************Account data persisted? " + success1);
                Console.WriteLine("**********************Skill data persisted? " + success2);
                Console.WriteLine("**********************Certs data persisted? " + success3);
                Console.WriteLine("**********************Edu data persisted? " + success4);
                Console.WriteLine("**********************Work data persisted? " + success5);
                Console.WriteLine("**********************Profile data persisted? " + success6);
            }
#endif
            /*
            if (SavedSkills.Count > 0)
            {
                DbColleagueSkillsRecord rd;
                Console.WriteLine(
                    new DbQueryString()
                    .Insert(rd = SavedSkills[1].ToDbRecord())
                    .AppendLine()
                    .SelectAll(rd.Table)
                    .AppendLine()
                    .SelectRequired(rd.Table.PrimaryKey)
                    .AppendLine()
                    .UpdateAll(rd.Table)
                    );
                Console.WriteLine(
                    new DbQueryString()
                    .SelectAll(SavedSkills[1].ToDbRecord().Table.PrimaryKey));
            }//*/
            return success;
        }


        /// <summary>
        /// This method will save, update, or delete and reload data to ensure all account data is updated in the database.
        /// </summary>
        /// <returns>true if the operation was successful, false otherwise.</returns>
        public bool Persist()
        {
            return Save();
        }


        public bool PersistSkills()
        {
            bool success = true;

            //persist all saved skill data
            foreach (SkillData d in SavedSkills.Values.ToList().Concat(UnsavedSkills))
            {
                //delete if remove, else save
                if (d.RecordID < 1 && d.Remove)
                    UnsavedSkills.Remove(d);
                else if (!d.Save())
                {
                    success = false;
                }
            }

            //load the new skill data

            Dictionary<int, SkillData> savedSkills;
            if (Program.DbRequestSystem)
            {
                object? result = AwaitDbResult(DbRequestManager.LoadColleagueSkills(this));
                if (result != null)
                {
                    savedSkills = (result as Dictionary<int, SkillData>);
                }
                else
                {
                    savedSkills = new Dictionary<int, SkillData>();
                }
            }
            else
            {
                if (!DatabaseConnector.LoadColleagueSkills1(this, out savedSkills))
                {
                    success = false;
                }
            }

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(savedSkills);
#if DEBUG
            Console.WriteLine("-------------Skills Saved " + UnsavedSkills.Count + ", Skills Loaded " + SavedSkills.Count);
#endif
            UnsavedSkills.Clear();

            return success;
        }


        public bool PersistEducationHistory()
        {
            bool success = true;

            //persist all saved education history data
            foreach (EducationData d in SavedEducationHistory.Values.Concat(UnsavedEducationHistory))
            {
                //delete if remove, else save
                if (d.RecordID < 1 && d.Remove)
                    UnsavedEducationHistory.Remove(d);
                else if (!d.Save())
                {
                    success = false;
                }
            }

            //load the new education history data

            Dictionary<int, EducationData> savedEducationHistory;
            if (Program.DbRequestSystem)
            {
                object? result = AwaitDbResult(DbRequestManager.LoadColleagueEducationHistory(this));
                if (result != null)
                {
                    savedEducationHistory = (result as Dictionary<int, EducationData>);
                }
                else
                {
                    savedEducationHistory = new();
                }
            }
            else
            {
                if (!DatabaseConnector.LoadColleagueEducationHistory1(this, out savedEducationHistory))
                {
                    success = false;
                }
            }

            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(savedEducationHistory);
#if DEBUG
            Console.WriteLine("-------------Education Saved " + UnsavedEducationHistory.Count + ", Education Loaded " + SavedEducationHistory.Count);
#endif
            UnsavedEducationHistory.Clear();

            return success;
        }


        public bool PersistCertifications()
        {
            bool success = true;

            //persist all saved education history data
            foreach (CertificationData d in SavedCertifications.Values.Concat(UnsavedCertifications))
            {
                if (d.RecordID < 1 && d.Remove)
                    UnsavedCertifications.Remove(d);
                else if (!d.Save())
                {
                    success = false;
                }
            }

            //load the new education history data

            Dictionary<int, CertificationData> savedCertifications;

            if (Program.DbRequestSystem)
            {
                object? result = AwaitDbResult(DbRequestManager.LoadColleagueCertifications(this));
                if (result != null)
                {
                    savedCertifications = (result as Dictionary<int, CertificationData>);
                }
                else
                {
                    savedCertifications = new();
                }
            }
            else
            {
                if (!DatabaseConnector.LoadColleagueCertifications(this, out savedCertifications))
                {
                    success = false;
                }
            }

            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(savedCertifications);
#if DEBUG
            Console.WriteLine("-------------Certifications Saved " + UnsavedCertifications.Count + ", Certifications Loaded " + SavedCertifications.Count);
#endif
            UnsavedCertifications.Clear();

            return success;
        }


        public bool PersistWorkHistory()
        {
            bool success = true;
            
            //persist all saved work history data
            foreach (WorkData d in SavedWorkHistory.Values.Concat(UnsavedWorkHistory))
            {

                if (d.RecordID < 1 && d.Remove)
                    UnsavedWorkHistory.Remove(d);
                else if (!d.Save())
                {
                    success = false;
                }

            }

            //load the new work history data

            Dictionary<int, WorkData> savedWorkHistory;

            if (Program.DbRequestSystem)
            {
                object? result = AwaitDbResult(DbRequestManager.LoadColleagueWorkHistory(this));
                if (result != null)
                {
                    savedWorkHistory = (Dictionary<int, WorkData>) result;
                }
                else
                {
                    savedWorkHistory = new();
                }
            }
            else
            {
                if (!DatabaseConnector.LoadColleagueWorkHistory1(this, out savedWorkHistory))
                {
                    success = false;
                }
            }

            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(savedWorkHistory);
#if DEBUG
            Console.WriteLine("-------------Work Saved " + UnsavedWorkHistory.Count + ", Work Loaded " + SavedWorkHistory.Count);
#endif
            UnsavedWorkHistory.Clear();

            return success;
        }


        public bool PersistProfiles()
        {
            bool success = true;

            //persist all saved profile data
            foreach (ProfileData d in SavedProfiles.Values.Concat(UnsavedProfiles))
            {

                //remove from unsaved if it was flagged for removal
                if (d.RecordID < 1 && d.Remove)
                    UnsavedProfiles.Remove(d);
                else if (!d.Save())
                {
                    success = false;
                }

            }

            //load the new profile data

            Dictionary<int, ProfileData> savedProfiles;

            if (Program.DbRequestSystem)
            {
                object? result = AwaitDbResult(DbRequestManager.LoadColleagueProfiles(this));
                if (result != null)
                {
                    savedProfiles = (Dictionary<int, ProfileData>)result;
                }
                else
                {
                    savedProfiles = new();
                }
            }
            else
            {
                if (!DatabaseConnector.LoadColleageProfiles(this, out savedProfiles))
                    success = false;
            }

            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(savedProfiles);

            int ValidateContentIDs<T>(ProfileData d, HashSet<int> idSet, ReadOnlyDictionary<int, T> savedRecords)
            {
                int removedIDs = 0;
                foreach (int id in idSet)
                    if (!savedRecords.ContainsKey(id))
                        if (idSet.Remove(id))
                            removedIDs++;
                return removedIDs;
            }

            int totalRemovedIDs = 0;
            //need to validate saved profile data ids, since they may have changed
            //some ids may have been removed, so we need to remove them from the saved profiles
            foreach (ProfileData d in SavedProfiles.Values)
            {
                totalRemovedIDs += ValidateContentIDs(d, d.SelectedSkillIDs, SavedSkills);
                totalRemovedIDs += ValidateContentIDs(d, d.SelectedEducationIDs, SavedEducationHistory);
                totalRemovedIDs += ValidateContentIDs(d, d.SelectedCertificationIDs, SavedCertifications);
                totalRemovedIDs += ValidateContentIDs(d, d.SelectedWorkIDs, SavedWorkHistory);
            }

#if DEBUG
            Console.WriteLine($"Removed a total of {totalRemovedIDs} IDs from profiles.");

            Console.WriteLine("-------------Profiles Saved " + UnsavedProfiles.Count + ", Profiles Loaded " + SavedProfiles.Count);
#endif
            UnsavedProfiles.Clear();

            return success;
        }

#endregion


        public DbAccountRecord ToDbRecord()
        {
            return new DbAccountRecord(this);
        }



        public override bool Equals(object obj)
        {
            if (obj == null || obj is not Account)
                return false;

            return GetUsername().Equals(((Account) obj).GetUsername());
        }
    }
}
