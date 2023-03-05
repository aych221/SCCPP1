using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;
using System.Security.Principal;
using System.Text;

namespace SCCPP1
{
    public class DatabaseConnector
    {

        private static string connStr = @"Data Source=CPPDatabse.db";

        //load these tables in memory to reduce CPU usage
        //may not need to do this
        private static Dictionary<int, string> skills = new Dictionary<int, string>();
        private static Dictionary<int, string> education_types = new Dictionary<int, string>();
        private static Dictionary<int, string> institutions = new Dictionary<int, string>();
        private static Dictionary<int, string> municipalities = new Dictionary<int, string>();

        //states are saved as abbreviation on first two chars and the other chars are the full name
        private static Dictionary<int, string> states = new Dictionary<int, string>();
        private static Dictionary<int, string> employers = new Dictionary<int, string>();
        private static Dictionary<int, string> job_titles = new Dictionary<int, string>();


        public static void CreateDatabase()
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                using (SqliteCommand cmd = new SqliteCommand(dbSQL, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            
        }


        #region Dictionary Loaders
        //Dictionary loaders
        public static void LoadSkills()
        {
            skills = LoadTwoColumnTable("skills");
        }

        public static void LoadEducationTypes()
        {
            education_types = LoadTwoColumnTable("education_types");
        }
        public static void LoadInstitutions()
        {
            institutions = LoadTwoColumnTable("institutions");
        }

        public static void LoadMunicipalities()
        {
            municipalities = LoadTwoColumnTable("municipalities");
        }

        public static void LoadStates()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT * FROM states;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        Dictionary<int, string> table = new Dictionary<int, string>();

                        char[] c = new char[2];
                        while (!r.Read())
                        {

                            //(column#, dataOffset, char array, buffer offset, char length)
                            r.GetChars(2, 0, c, 0, 2);

                            //loads the ID as the key and the value as the string
                            table.TryAdd(GetInt32(r, 0), c + "" + GetString(r, 1));
                        }

                        states = table;
                    }
                }
            }
        }

        public static void LoadEmployers()
        {
            employers = LoadTwoColumnTable("employers");
        }

        public static void LoadJobTitles()
        {
            job_titles = LoadTwoColumnTable("job_titles");
        }


        private static Dictionary<int, string> LoadTwoColumnTable(string tableName)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT * FROM @table;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@table", tableName);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        Dictionary<int, string> table = new Dictionary<int, string>();

                        while (!r.Read())
                        {
                            //loads the ID as the key and the value as the string
                            table.TryAdd(GetInt32(r, 0), GetString(r, 1));
                        }

                        return table;
                    }
                }
            }
        }
        #endregion


        #region Dictionary Getters
        //Dictionary getters
        public static string? GetCachedSkill(int id)
        {
            return GetCachedValue(id, skills);
        }

        //first searches the dictionary for the skill,
        //if not found, it will search the database
        //if nothing is found, it will return null.
        public static string? TryGetSkill(int id)
        {
            string? s;
            if ((s = GetCachedSkill(id)) == null)
                return GetCachedSkill(id);

            return s;
        }

        public static string? GetCachedEducationType(int id)
        {
            return GetCachedValue(id, education_types);
        }

        public static string? GetCachedInstitution(int id)
        {
            return GetCachedValue(id, institutions);
        }

        public static string? GetCachedMunicipality(int id)
        {
            return GetCachedValue(id, municipalities);
        }

        public static string? GetCachedState(int id)
        {
            return GetCachedValue(id, states);
        }

        public static string? GetCachedEmployer(int id)
        {
            return GetCachedValue(id, employers);
        }
        
        public static string? GetCachedJobTitle(int id)
        {
            return GetCachedValue(id, job_titles);
        }


        private static string? GetCachedValue(int id, Dictionary<int, string> table)
        {
            string? s;

            if (table.TryGetValue(id, out s))
                return s;

            return null;
        }
        #endregion

        #region User Data
        /// <summary>
        /// Loads a new Account object into the SessionData provided, if the user exists.
        /// </summary>
        /// <param name="sessionData">The current session object</param>
        /// <returns>true if the account exists, false if the account does not exist</returns>
        [Obsolete]
        public static bool LoadUserData(SessionData sessionData)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, email, name FROM colleagues WHERE (user_hash=@user);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(sessionData.Username));
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return false;


                        //load new instance with basic colleague information
                        Account a = new Account(sessionData, true);

                        a.RecordID = GetInt32(r, 0);
                        a.Role = GetInt32(r, 1);
                        a.Name = GetString(r, 2);
                        a.Email = GetString(r, 3);

                        sessionData.Owner = a;

                        return true;
                    }
                }
            }
        }


        public static int InsertUser(string userID, int role, string name, string email, int phone, string address, string introNarrative, int mainProfileID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO colleagues (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id) VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id) RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(phone));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(address));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(introNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(mainProfileID));

                    object? accountID = cmd.ExecuteScalar();

                    if (accountID == null)
                        return -1;

                    return Convert.ToInt32(accountID);//return record ID

                }
            }
        }



        public static int InsertUser(Account account)
        {
            return InsertUser(account.GetUsername(), account.Role, account.Name, account.Email, account.Phone, account.Address, account.IntroNarrative, account.MainProfileID);
        }

        public static int UpdateUser(int id, string userID, int role, string name, string email, int phone, string address, string introNarrative, int mainProfileID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE colleagues SET user_hash=@user_hash, role=@role, name=@name, email=@email, phone=@phone, address=@address, intro_narrative=@intro_narrative, main_profile_id=@main_profile_id WHERE id = @id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(phone));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(address));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(introNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(mainProfileID));

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static int UpdateUser(Account account)
        {
            return UpdateUser(account.RecordID, account.GetUsername(), account.Role, account.Name, account.Email, account.Phone, account.Address, account.IntroNarrative, account.MainProfileID);
        }


        //used to save an account if you already have an id.
        //just to be save, it still checks to see if the user exists.
        //put -1 if id is unknown
        public static bool SaveUser(int id, string userID, int role, string name, string email, int phone, string address, string introNarrative, int mainProfileID)
        {
            if (!ExistsUser(id))
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
        }

        //used if you don't have an ID
        //put -1 if id is unknown
        public static int SaveUser(string userID, int role, string name, string email, int phone, string address, string introNarrative, int mainProfileID)
        {
            int id = ExistsUser(userID);
            if (id == -1)
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID);
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID);
        }

        //used to save an account object, this will determine if the user needs to be created or not
        public static bool SaveUser(Account account)
        {
            if (account.IsReturning)
                return SaveUser(account.RecordID, account.GetUsername(), account.Role, account.Name, account.Email, account.Phone, account.Address, account.IntroNarrative, account.MainProfileID);
            
            return (account.RecordID = SaveUser(account.GetUsername(), account.Role, account.Name, account.Email, account.Phone, account.Address, account.IntroNarrative, account.MainProfileID)) >= 0;
        }


        public static bool ExistsUser(int id)
        {
            if (id < 0)
                return false;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM colleagues WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return true;
                        return false;
                    }
                }
            }
        }

        //exists by userID (the hashed user)
        public static int ExistsUser(string userID)
        {
            if (userID == null)
                return -1;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM colleagues WHERE (user_hash=@user_hash);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);
                        return -1;
                    }
                }
            }
        }

        public static int GetUserRecordID(string username)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, email, name FROM colleagues WHERE (user_hash=@user);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(username));
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return -1;

                        return GetInt32(r, 0);
                    }
                }
            }
        }


        #region debug users
        //old code from my other db
        private static void printusers()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT *  FROM colleagues;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Console.WriteLine($"'{GetString(r, 1)}', '{GetString(r, 2)}'");//user
                        }

                    }
                }
            }
        }
        #endregion


        public static Account? GetUser(string userID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM colleagues WHERE (user_hash=@user_hash);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_hash", userID);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return null;


                        //load new instance with basic colleague information
                        Account a = new Account(null, true);

                        a.RecordID = GetInt32(r, 0);
                        a.Role = GetInt32(r, 1);
                        a.Name = GetString(r, 2);
                        a.Email = GetString(r, 3);
                        a.Phone = GetInt32(r, 4);
                        a.Address = GetString(r, 5);
                        a.IntroNarrative = GetString(r, 6);
                        a.MainProfileID = GetInt32(r, 7);

                        return a;
                    }
                }
            }
        }


        //loads user based on ID into an Account, likely will be used by admins
        public static Account? GetUser(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM colleagues WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return null;


                        //load new instance with basic colleague information
                        Account a = new Account(null, true);

                        a.RecordID = GetInt32(r, 1);
                        a.Role = GetInt32(r, 2);
                        a.Name = GetString(r, 3);
                        a.Email = GetString(r, 4);
                        a.Phone = GetInt32(r, 5);
                        a.Address = GetString(r, 6);
                        a.IntroNarrative = GetString(r, 7);
                        a.MainProfileID = GetInt32(r, 8);

                        return a;
                    }
                }
            }
        }
        #endregion


        #region Skill Data

        //TODO, do we need to care about case?
        public static int SaveSkill(string skillName)
        {
            int id = GetSkillID(skillName);

            if (id == -1)
                return InsertSkill(skillName);

            return id;
        }

        public static int[] SaveSkills(params string[] skillNames)
        {
            int[] skillIDs;

            //return if all skills were found and "saved"
            if (GetSkillIDs(out skillIDs, skillNames))
                return skillIDs;
            
            //else, we need to add the missing skills
            StringBuilder skills = new StringBuilder();

            for (int i = 0; i < skillIDs.Length; i ++)
                if (skillIDs[i] == -1)
                    skills.Append($"{skillNames[i]}");

            //not recommended to concat arrays, but these may be relatively small
            //might end up using ArrayLists
            return skillIDs.Concat(InsertSkills(skills.ToString())).ToArray();

        }

        /// <summary>
        /// Inserts a new skill to the database
        /// </summary>
        /// <param name="skillName"></param>
        /// <returns>-1 if the skill was not added, otherwise returns the id that was given to the record</returns>
        public static int InsertSkill(string skillName)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO skills(name) VALUES (@name) RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", skillName);


                    object? skillID = cmd.ExecuteScalar();

                    if (skillID == null)
                        return -1;

                    return Convert.ToInt32(skillID);//return record ID
                }
            }
        }

        
        public static int[] InsertSkills(params string[] skillNames)
        {
            int[] insertedSkillIDs = new int[skillNames.Length];

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //not sure if this is the most efficient solution for this table
                //will need to run tests
                string sql = @"INSERT OR IGNORE INTO skills(name) VALUES (@name);
                       SELECT id FROM skills WHERE name = @name;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    for (int i = 0; i < skillNames.Length; i++)
                    {
                        cmd.Parameters.Clear(); //ensure @name is cleared and updated.
                        cmd.Parameters.AddWithValue("@name", skillNames[i]);

                        object? skillID = cmd.ExecuteScalar();

                        if (skillID != null)
                            insertedSkillIDs[i] = Convert.ToInt32(skillID);
                        else
                            insertedSkillIDs[i] = InsertSkill(skillNames[i]);
                    }
                }
            }

            return insertedSkillIDs;
        }


        /// <summary>
        /// Retrieves the skill name based on the id.
        /// </summary>
        /// <param name="id">the skill id</param>
        /// <returns>null if skill id does not exist, otherwise returns skill name</returns>
        public static string? GetSkillName(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name FROM skills WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetString(r, 0);

                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the ID for a skill if it exists.
        /// </summary>
        /// <param name="skillName"></param>
        /// <returns>-1 if the skills was not found, otherwise returns the skill record ID</returns>
        public static int GetSkillID(string skillName)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM skills WHERE (name=@name);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", skillName);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);

                        return -1;
                    }
                }
            }
        }

        //will work great for large amounts of skills, not so sure if it's better for small amount of skills
        public static int[] GetSkillIDs(params string[] skillNames)
        {
            Dictionary<string, int> skillNameResults = new Dictionary<string, int>(skillNames.Length);
            int[] skillIDs = new int[skillNames.Length];

            //create skills list
            StringBuilder skillsList = new StringBuilder(skillNames[0]);

            for (int i = 1; i < skillNames.Length; i ++)
            {
                skillsList.Append($", {skillNames[i]}");

                //fill with -1's initially to indicate not found
                skillNameResults.Add(skillNames[i], -1);
            }

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name, id FROM skills WHERE name IN (@skillsList)";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@skillsList", skillsList.ToString());
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            skillNameResults[GetString(r, 0)] = GetInt32(r, 1);

                        for (int i = 0; i < skillNames.Length; i++)
                            skillIDs[i] = skillNameResults[skillNames[i]];

                        return skillIDs;
                    }
                }
            }
        }

        public static bool GetSkillIDs(out int[] skillIDs, params string[] skillNames)
        {
            //is set to true if any one of the records ends up being -1
            bool hasMissingRecord = false;

            Dictionary<string, int> skillNameResults = new Dictionary<string, int>(skillNames.Length);
            skillIDs = new int[skillNames.Length];

            //create skills list
            StringBuilder skillsList = new StringBuilder(skillNames[0]);

            for (int i = 1; i < skillNames.Length; i++)
            {
                skillsList.Append($", {skillNames[i]}");

                //fill with -1's initially to indicate not found
                skillNameResults.Add(skillNames[i], -1);
            }

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name, id FROM skills WHERE name IN (@skillsList)";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@skillsList", skillsList.ToString());
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            skillNameResults[GetString(r, 0)] = GetInt32(r, 1);

                        for (int i = 0; i < skillNames.Length; i++)
                        {
                            if ((skillIDs[i] = skillNameResults[skillNames[i]]) == -1)
                                hasMissingRecord = true;
                        }

                        //returns true if all records were found, false if there was a missing record
                        return !hasMissingRecord;
                    }
                }
            }
        }

        /*
        public static int[] GetSkillIDs(params string[] skillNames)
        {
            int[] insertedSkillIDs = new int[skillNames.Length];

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //not sure if this is the most efficient solution for this table
                //will need to run tests
                string sql = @"SELECT id FROM skills WHERE (name = @name);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    for (int i = 0; i < skillNames.Length; i++)
                    {
                        cmd.Parameters.Clear(); //ensure @name is cleared and updated.
                        cmd.Parameters.AddWithValue("@name", skillNames[i]);

                        object? skillID = cmd.ExecuteScalar();

                        if (skillID != null)
                            insertedSkillIDs[i] = Convert.ToInt32(skillID);
                        else
                            insertedSkillIDs[i] = InsertSkill(skillNames[i]);
                    }
                }
            }

            return insertedSkillIDs;
        }*/

        //TODO: may need to change skills to be a csv string
        /// <summary>
        /// Returns all of the saved colleague skills.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <returns>a list of strings that stores the colleague_skill_id\skill_id\rating in that format</returns>
        public static List<string>? GetRawColleagueSkills(Account account)
        {
            //session must've not had an account, so user must not exist
            if (account == null)
                return null;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT * FROM colleague_skills WHERE (colleague_id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", account.RecordID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        string s;
                        while (r.Read())
                        {
                            s = GetInt32(r, 0) + "\\"; //colleage skill id
                            s += GetInt32(r, 2) + "\\"; //skill id
                            s += GetInt32(r, 3) + ""; //rating for skill

                            list.Add(s);
                        }

                        return list;
                    }
                }
            }
        }
        #endregion


        #region Education Data

        public static int InsertEducationHistory(int colleagueID, int educationTypeID, int institutionID, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                education_history(colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description) 
                                VALUES (@colleague_id, @education_type_id, @institution_id, @municipality_id, @state_id, @start_date, @end_date, @description)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@education_type_id", educationTypeID);
                    cmd.Parameters.AddWithValue("@institution_id", institutionID);
                    cmd.Parameters.AddWithValue("@municipality_id", municipalityID);
                    cmd.Parameters.AddWithValue("@state_id", stateID);
                    cmd.Parameters.AddWithValue("@start_date", startDate);
                    cmd.Parameters.AddWithValue("@end_date", endDate);
                    cmd.Parameters.AddWithValue("@description", description);


                    object? educationID = cmd.ExecuteScalar();

                    if (educationID == null)
                        return -1;

                    return Convert.ToInt32(educationID);//return record ID
                }
            }
        }


        public static int InsertEducationHistory(EducationData ed)
        {
            return InsertEducationHistory(ed.Owner.RecordID, ed.EducationTypeID, ed.InstitutionID, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }

        public static int UpdateEducationHistory(int id, int colleagueID, int educationTypeID, int institutionID, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE education_history SET 
                                colleague_id=@colleague_id, education_type_id=@education_type_id, institution_id=@institution_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description WHERE id = @id;";
                
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@education_type_id", educationTypeID);
                    cmd.Parameters.AddWithValue("@institution_id", institutionID);
                    cmd.Parameters.AddWithValue("@municipality_id", municipalityID);
                    cmd.Parameters.AddWithValue("@state_id", stateID);
                    cmd.Parameters.AddWithValue("@start_date", startDate);
                    cmd.Parameters.AddWithValue("@end_date", endDate);
                    cmd.Parameters.AddWithValue("@description", description);
                    return cmd.ExecuteNonQuery();//rows affected
                }
            }
        }

        public static int UpdateEducationHistory(EducationData ed)
        {
            return UpdateEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationTypeID, ed.InstitutionID, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }


        //put -1 if id is unknown
        public static bool SaveEducationHistory(int id, int colleagueID, string educationType, string institutionName, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            int educationTypeID = SaveEducationType(educationType),
                institutionID = SaveInstitution(institutionName);
            Thread.Sleep(1);
            if (ExistsEducationHistory(id))
                return UpdateEducationHistory(id, colleagueID, educationTypeID, institutionID, municipalityID, stateID, startDate, endDate, description) >= 0;
            return InsertEducationHistory(colleagueID, educationTypeID, institutionID, municipalityID, stateID, startDate, endDate, description) >= 0;
        }

        public static bool SaveEducationHistory(EducationData ed)
        {
            return SaveEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationType, ed.Institution, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }

        #region Education Type
        public static int GetEducationTypeID(string educationType)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM education_types WHERE (type=@type);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@type", educationType);

                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);

                        return -1;
                    }
                }
            }
        }

        public static string? GetEducationType(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT type FROM education_types WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetString(r, 0);

                        return null;
                    }
                }
            }
        }


        public static int InsertEducationType(string type)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                education_types(type) 
                                VALUES (@type)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@type", type);


                    object? educationTypeID = cmd.ExecuteScalar();

                    if (educationTypeID == null)
                        return -1;

                    return Convert.ToInt32(educationTypeID);//return record ID
                }
            }

        }

        //TODO, do we need to care about case?
        public static int SaveEducationType(string type)
        {
            int id = GetEducationTypeID(type);

            if (id < 0)
                return InsertEducationType(type);

            return id;
        }
        #endregion


        #region Institutions
        public static string? GetInstitution(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name FROM institutions WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetString(r, 0);

                        return null;
                    }
                }
            }
        }

        public static int GetInstitutionID(string institutionName)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM institutions WHERE (name=@name);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", institutionName);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);

                        return -1;
                    }
                }
            }
        }

        public static int InsertInstitution(string name)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                institutions(name) 
                                VALUES (@name)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);


                    object? institutionID = cmd.ExecuteScalar();

                    if (institutionID == null)
                        return -1;

                    return Convert.ToInt32(institutionID);//return record ID
                }
            }

        }

        //TODO, do we need to care about case?
        public static int SaveInstitution(string name)
        {
            int id = GetInstitutionID(name);

            if (id < 0)
                return InsertInstitution(name);

            return id;
        }
        #endregion


        public static bool ExistsEducationHistory(int id)
        {
            if (id < 0)
                return false;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM education_history WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return true;
                        return false;
                    }
                }
            }
        }

        public static List<string> GetRawColleagueEducationHistory(int colleagueID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        Console.WriteLine($"Attempting to fetch Education record for {colleagueID}");
                        string s;
                        while (r.Read())
                        {
                            s = GetInt32(r, 0) + "\\"; //record id
                            s += GetInt32(r, 2) + "\\"; //education type id
                            s += GetInt32(r, 3) + "\\"; //institution id
                            s += GetInt32(r, 4) + "\\"; //municipality id
                            s += GetInt32(r, 5) + "\\"; //state id

                            s += GetDateOnly(r, 6).ToString() + "\\"; //start date
                            s += GetDateOnly(r, 7).ToString() + "\\"; //end date (might be empty)

                            s += GetString(r, 8) + "\\"; //description (might be empty)
                            Console.WriteLine(GetInt32(r, 0) + " record");

                            list.Add(s);
                        }

                        return list;
                    }
                }
            }
        }


        public static List<EducationData>? GetColleagueEducationHistory(Account account)
        {
            if (account == null || account.RecordID < 0)
                return null;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<EducationData> list = new List<EducationData>();

                        while (r.Read())
                        {
                            EducationData ed = new EducationData(account, GetInt32(r, 0));
                            ed.EducationTypeID = GetInt32(r, 2);
                            ed.InstitutionID = GetInt32(r, 3);
                            ed.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));

                            ed.StartDate = GetDateOnly(r, 6);
                            ed.EndDate = GetDateOnly(r,7);

                            ed.Description = GetString(r, 8);

                            list.Add(ed);
                        }

                        return list;
                    }
                }
            }
        }

        #endregion


        #region Work Data
        //TODO make query fields dynamic in what is typed in
        public static int InsertWorkHistory(params InputF[] fields)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string? sql = $"{QueryGeneratorInsert("work_history", fields)} RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    foreach (InputF f in fields)
                        cmd.Parameters.AddWithValue(f.VariableName, f.Value);
                    /*cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@employer_id", employerID);
                    cmd.Parameters.AddWithValue("@job_title_id", jobTitleID);
                    cmd.Parameters.AddWithValue("@municipality_id", municipalityID);
                    cmd.Parameters.AddWithValue("@state_id", stateID);
                    cmd.Parameters.AddWithValue("@start_date", startDate);
                    cmd.Parameters.AddWithValue("@end_date", endDate);
                    cmd.Parameters.AddWithValue("@description", description);*/


                    object? workID = cmd.ExecuteScalar();

                    if (workID == null)
                        return -1;

                    return Convert.ToInt32(workID);//return record ID
                }
            }
        }//*/

        public static int InsertWorkHistory(int colleagueID, int employerID, int jobTitleID, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                work_history(colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description) 
                                VALUES (@colleague_id, @employer_id, @job_title_id, @municipality_id, @state_id, @start_date, @end_date, @description)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@employer_id", employerID);
                    cmd.Parameters.AddWithValue("@job_title_id", jobTitleID);
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(municipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(stateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(startDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(endDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(description));

                    Console.WriteLine("Saving Work History...");
                    //TODO fix empty values
                    object? workID = cmd.ExecuteScalar();

                    if (workID == null)
                        return -1;

                    Console.WriteLine("Saved Work History!");
                    return Convert.ToInt32(workID);//return record ID
                }
            }
        }

        public static int InsertWorkHistory(WorkData wd)
        {
            return InsertWorkHistory(wd.Owner.RecordID, wd.EmployerID, wd.JobTitleID, wd.Location.MunicipalityID, wd.Location.StateID, wd.StartDate, wd.EndDate, wd.Description);
        }


        public static int UpdateWorkHistory(int id, int colleagueID, int employerID, int jobTitleID, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE work_history SET 
                                colleague_id=@colleague_id, employer_id=@employer_id, job_title_id=@job_title_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description WHERE id = @id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@employer_id", employerID);
                    cmd.Parameters.AddWithValue("@job_title_id", jobTitleID);
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(municipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(stateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(startDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(endDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(description));

                    return cmd.ExecuteNonQuery();//rows affected
                }
            }
        }


        public static int UpdateWorkHistory(WorkData wd)
        {
            return UpdateWorkHistory(wd.RecordID, wd.Owner.RecordID, wd.EmployerID, wd.JobTitleID, wd.Location.MunicipalityID, wd.Location.StateID, wd.StartDate, wd.EndDate, wd.Description);
        }

        public static bool ExistsWorkHistory(int id)
        {
            if (id < 0)
                return false;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM work_history WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return true;
                        return false;
                    }
                }
            }
        }




        //put -1 if id is unknown
        public static bool SaveWorkHistory(int id, int colleagueID, string employer, string jobTitle, int municipalityID, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            int employerID = SaveEmployer(employer),
                jobTitleID = SaveJobTitle(jobTitle);

            if (ExistsWorkHistory(id))
                return UpdateWorkHistory(id, colleagueID, employerID, jobTitleID, municipalityID, stateID, startDate, endDate, description) >= 0;
            return InsertWorkHistory(colleagueID, employerID, jobTitleID, municipalityID, stateID, startDate, endDate, description) >= 0;
        }

        public static bool SaveWorkHistory(WorkData wd)
        {
            wd.EmployerID = SaveEmployer(wd.Employer);
            wd.JobTitleID = SaveJobTitle(wd.JobTitle);

            if (ExistsWorkHistory(wd.RecordID))
                return (wd.RecordID = UpdateWorkHistory(wd)) >= 0;

            return (wd.RecordID = InsertWorkHistory(wd)) >= 0;
        }

        #region Employer
        public static int GetEmployerID(string employer)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM employers WHERE (name=@name);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", employer);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);

                        return -1;
                    }
                }
            }
        }

        public static string? GetEmployer(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name FROM employers WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetString(r, 0);

                        return null;
                    }
                }
            }
        }


        public static int InsertEmployer(string name)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                employers(name) 
                                VALUES (@name)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    Console.WriteLine("Inserted:");

                    object? employerID = cmd.ExecuteScalar();

                    if (employerID == null)
                        return -1;

                    return Convert.ToInt32(employerID);//return record ID
                }
            }

        }

        //TODO, do we need to care about case?
        public static int SaveEmployer(string name)
        {
            int id = GetEmployerID(name);

            if (id < 0)
                return InsertEmployer(name);

            return id;
        }
        #endregion


        #region Job Title
        public static string? GetJobTitle(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT title FROM job_titles WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetString(r, 0);

                        return null;
                    }
                }
            }
        }

        public static int GetJobTitleID(string title)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM job_titles WHERE (title=@title);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (r.Read())
                            return GetInt32(r, 0);

                        return -1;
                    }
                }
            }
        }


        public static int InsertJobTitle(string title)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                job_titles(title) 
                                VALUES (@title)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);


                    object? jobTitleID = cmd.ExecuteScalar();

                    if (jobTitleID == null)
                        return -1;

                    return Convert.ToInt32(jobTitleID);//return record ID
                }
            }

        }

        //TODO, do we need to care about case?
        public static int SaveJobTitle(string title)
        {
            int id = GetJobTitleID(title);

            if (id < 0)
                return InsertJobTitle(title);

            return id;
        }
        #endregion




        public static List<string> GetRawColleagueWorkHistory(int colleagueID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description FROM work_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        Console.WriteLine($"Attempting to fetch Work record for {colleagueID}");
                        string s;
                        while (r.Read())
                        {
                            s = GetInt32(r, 0) + "\\"; //record id
                            s += GetInt32(r, 2) + "\\"; //employer id
                            s += GetInt32(r, 3) + "\\"; // job title id
                            s += GetInt32(r, 4) + "\\"; //municipality id
                            s += GetInt32(r, 5) + "\\"; //state id

                            s += r.GetDateTime(6) + "\\"; //start date
                            s += r.GetDateTime(7) + "\\"; //end date (might be empty)

                            s += GetString(r, 8) + ""; //description (might be empty)

                            list.Add(s);
                        }

                        return list;
                    }
                }
            }
        }


        public static List<WorkData>? GetColleagueWorkHistory(Account account)
        {
            if (account == null || account.RecordID < 0)
                return null;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<WorkData> list = new List<WorkData>();

                        while (r.Read())
                        {
                            WorkData wd = new WorkData(account, GetInt32(r, 0));
                            wd.EmployerID = GetInt32(r, 2);
                            wd.JobTitleID = GetInt32(r, 3);
                            wd.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));

                            wd.StartDate = Utilities.ToDateOnly(r.GetDateTime(6));
                            wd.EndDate = Utilities.ToDateOnly(r.GetDateTime(7));

                            wd.Description = GetString(r, 8);

                            list.Add(wd);
                        }

                        return list;
                    }
                }
            }
        }
        #endregion


        #region Profile Methods


        #endregion
        public static List<string> PrintRecords(string table)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = $"SELECT * FROM {table};";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        Console.WriteLine($"Attempting to fetch {table} records...");
                        string s;
                        while (r.Read())
                        {
                            s = "";
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                r.GetOrdinal(r.GetName(i));
                                s += $"{r.GetOrdinal(r.GetName(i))}:{r[i]}\\";
                            }
                            list.Add(s);
                        }


                        return list;
                    }
                }
            }
        }

        #region Debugging

        #endregion
        public static void TestBrittany()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //colleague insertion
                using (SqliteCommand cmd = new SqliteCommand("INSERT OR IGNORE INTO colleagues (user_hash, role, name, phone, address, intro_narrative) VALUES (@user_hash, @role, @name, @phone, @address, @intro_narrative)", conn))
                {
                    // all emails will be lowercased to ensure hash consistency
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash("BrittL".ToLower()));
                    // normal user
                    cmd.Parameters.AddWithValue("@role", 1);
                    cmd.Parameters.AddWithValue("@name", "Brittany Langosh");
                    cmd.Parameters.AddWithValue("@phone", "555-555-5555");
                    cmd.Parameters.AddWithValue("@address", "123 Hopper Lane");
                    cmd.Parameters.AddWithValue("@intro_narrative", "Brittany Langosh has been a Program Manager for the past 12 years. She has extensive experience in...");
                    cmd.ExecuteNonQuery();
                }

            }

        }


        public static void TestBrittany1()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //colleague insertion
                using (SqliteCommand cmd = new SqliteCommand("INSERT OR IGNORE INTO colleagues (user_hash, role, name, phone, address, intro_narrative) VALUES (@user_hash, @role, @name, @phone, @address, @intro_narrative)", conn))
                {
                    // all emails will be lowercased to ensure hash consistency
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash("BrittL".ToLower()));
                    // normal user
                    cmd.Parameters.AddWithValue("@role", 1);
                    cmd.Parameters.AddWithValue("@name", "Brittany Langosh");
                    cmd.Parameters.AddWithValue("@phone", "555-555-5555");
                    cmd.Parameters.AddWithValue("@address", "123 Hopper Lane");
                    cmd.Parameters.AddWithValue("@intro_narrative", "Brittany Langosh has been a Program Manager for the past 12 years. She has extensive experience in...");
                    cmd.ExecuteNonQuery();
                }

            }

        }

        public static void SaveBrittany(Account account)
        {
            account.Name = "Brittany Langosh";
            account.Email = "lbrittany02@rsi.com";
            account.Phone = 1231231234;
            account.Address = "123 Main St";
            account.IntroNarrative = "Brittany Langosh has been a Product Manager...";

            EducationData ed1 = new EducationData(account),
                ed2 = new EducationData(account),
                ed3 = new EducationData(account);

            ed1.EducationType = "Masters of Business Administration";
            ed1.Description = "in Crypto";
            ed1.Institution = "Harvard";
            ed1.StartDate = Utilities.ToDateOnly("August 18 1993");

            ed2.EducationType = "Bachelor of Arts";
            ed2.Description = "in Computer Design";
            ed2.Institution = "Aiken Technical College";
            ed2.StartDate = Utilities.ToDateOnly("August 15 1990");

            ed3.EducationType = "Scrum Master";
            ed3.Description = "in my Mom's Kitchen";
            ed3.Institution = "North Augusta High School";
            ed3.StartDate = Utilities.ToDateOnly("August 8 1980");


            WorkData wd1 = new WorkData(account, 1),
                wd2 = new WorkData(account, 2);

            wd1.Employer = "The Kern Family Foundation";
            wd1.JobTitle = "Chair Sitting Assistant";
            wd1.StartDate = Utilities.ToDateOnly("September 1 2006");
            wd1.EndDate = Utilities.ToDateOnly("September 1 2009");

            wd2.Employer = "Management Research Service";
            wd2.JobTitle = "Social Media Influencer";
            wd2.StartDate = Utilities.ToDateOnly("October 12 2009");
            wd2.EndDate = Utilities.ToDateOnly("April 1 2023");


            SkillData sd1 = new SkillData(account),
                sd2 = new SkillData(account),
                sd3 = new SkillData(account);

            sd1.Skill = "Java";
            sd2.Skill = "C#";
            sd3.Skill = "HTML";

            SaveUser(account);
            //SaveSkill(")
            SaveWorkHistory(wd1);
            SaveWorkHistory(wd2);
            SaveEducationHistory(ed1);
            SaveEducationHistory(ed2);
            SaveEducationHistory(ed3);
            Console.WriteLine("Saved!");
            foreach (string s in GetRawColleagueEducationHistory(account.RecordID))
                Console.WriteLine(s);
            foreach (string s in GetRawColleagueWorkHistory(account.RecordID))
                Console.WriteLine(s);

            foreach (string s in PrintRecords("work_history"))
                Console.WriteLine(s);
            foreach (string s in PrintRecords("education_history"))
                Console.WriteLine(s);
            foreach (string s in PrintRecords("colleagues"))
                Console.WriteLine(s);
        }
        public static void TestBrittany2()
        {


            Account account = new Account(null, false);
            account.Name = "Brittany Langosh";
            account.Email = "lbrittany02@rsi.com";
            account.Phone = 1231231234;
            account.Address = "123 Main St";
            account.IntroNarrative = "Brittany Langosh has been a Product Manager...";
            //account.MainProfileID = 0;

            EducationData ed1 = new EducationData(account, 1),
                ed2 = new EducationData(account, 2),
                ed3 = new EducationData(account, 3);

            ed1.EducationType = "Masters of Business Administration";
           
            ed1.Description = "in Crypto";
            ed1.Institution = "Harvard";
            ed1.StartDate = Utilities.ToDateOnly("August 18 1993");
            ed1.EndDate = Utilities.ToDateOnly("August 18 2000");

            ed2.EducationType = "Bachelor of Arts";
            ed2.Description = "in Computer Design";
            ed2.Institution = "Aiken Technical College";
            ed2.StartDate = Utilities.ToDateOnly("August 15 1990");
            ed2.EndDate = Utilities.ToDateOnly("August 18 1993");

            ed3.EducationType = "Scrum Master";
            ed3.Description = "in my Mom's Kitchen";
            ed3.Institution = "North Augusta High School";
            ed3.StartDate = Utilities.ToDateOnly("August 8 1980");


            WorkData wd1 = new WorkData(account, 1),
                wd2 = new WorkData(account, 2);

            wd1.Employer = "The Kern Family Foundation";
            wd1.JobTitle = "Chair Sitting Assistant";
            wd1.StartDate = Utilities.ToDateOnly("September 1 2006");
            wd1.EndDate = Utilities.ToDateOnly("September 1 2009");

            wd2.Employer = "Management Research Service";
            wd2.JobTitle = "Social Media Influencer";
            wd2.StartDate = Utilities.ToDateOnly("October 12 2009");
            wd2.EndDate = Utilities.ToDateOnly("April 1 2023");


            SkillData sd1 = new SkillData(account, 1),
                sd2 = new SkillData(account, 2),
                sd3 = new SkillData(account, 3);

            sd1.Skill = "Java";
            sd2.Skill = "C#";
            sd3.Skill = "HTML";

            /*InsertUser("Britt", 0, "Brittany Langosh", "Britt@noemail.com", 1231231234, "123 Main St", "Brittany Langosh has been a Product Manager...", 0);
            InsertEducationHistory(0, 0, 0, 0, 0, Utilities.ToDateOnly("July 1 2022"), Utilities.ToDateOnly("July 5 2022"), "Masters of Business Admin");
            InsertEducationHistory(0, 0, 0, 0, 0, Utilities.ToDateOnly("July 1 2022"), Utilities.ToDateOnly("July 5 2022"), "Bachelor of Arts");
            InsertEducationHistory(0, 0, 0, 0, 0, Utilities.ToDateOnly("July 1 2022"), Utilities.ToDateOnly("July 5 2022"), "Scrum Master");
            InsertSkill("Java");
            InsertSkill("C#");
            InsertSkill("HTML");
            //InsertColleageSkills(0, 0, 10);
            //InsertColleageSkills(0, 0, 6);
            //missing insert munic, states, institutions
            InsertWorkHistory(0, 0, 0, 0, 0, Utilities.ToDateOnly("July 1 2022"), Utilities.ToDateOnly("July 5, 2022"), "The Kern Family Foundation");
            InsertWorkHistory(0, 0, 0, 0, 0, Utilities.ToDateOnly("July 1 2022"), Utilities.ToDateOnly("July 5, 2022"), "Management Research Services");*/


        }

        private static object ValueCleaner(int val)
        {
            if (val == 0)
                return DBNull.Value;

            return val;
        }

        private static object ValueCleaner(string val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }

        private static object ValueCleaner(DateOnly? val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }

        private static int GetInt32(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt32(ordinal);
        }

        private static string GetString(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return null;
            return r.GetString(ordinal);
        }

        private static DateOnly GetDateOnly(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return DateOnly.MinValue;
            return Utilities.ToDateOnly(r.GetDateTime(ordinal));
        }

        /*private static void AddWithValueOrNull(SqliteCommand cmd, string variable, object? value)
        {
            if (value == null)
        }*/

        //TODO make query fields dynamic and optional for calling.
        /*private static string GenerateQueryFields()
        {
            StringBuilder sb = new StringBuilder();
        }*/


        /*
		 * 
		USE master;
		ALTER DATABASE MovieSchedulingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

		DROP DATABASE IF EXISTS MovieSchedulingDB;


		CREATE DATABASE MovieSchedulingDB;

		USE MovieSchedulingDB;*/


        ///<summary>
        ///This is initial query used for creating the initial database and tables.
        ///Should only be used if the database is being completely reset or initially created.
        ///</summary>
        private const string dbSQL = @"

        BEGIN TRANSACTION; 
        DROP TABLE IF EXISTS [colleagues];
        DROP TABLE IF EXISTS [municipalities];
        DROP TABLE IF EXISTS [states];
        DROP TABLE IF EXISTS [education_types];
        DROP TABLE IF EXISTS [institutions];
        DROP TABLE IF EXISTS [education_history];
        DROP TABLE IF EXISTS [employers];
        DROP TABLE IF EXISTS [job_titles];
        DROP TABLE IF EXISTS [work_history];
        DROP TABLE IF EXISTS [skills];
        DROP TABLE IF EXISTS [colleague_skills];
        DROP TABLE IF EXISTS [profiles];
        COMMIT;

        CREATE TABLE colleagues (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          user_hash TEXT NOT NULL,
          role INTEGER NOT NULL, --0=admin 1=normal
          name TEXT NOT NULL,
          email TEXT,
          phone INTEGER,
          address TEXT,
          intro_narrative TEXT
        );

        CREATE TABLE municipalities (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL
        );

        CREATE TABLE states (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL,
          abbreviation CHAR(2)
        );

        CREATE TABLE institutions (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL
        );

        CREATE TABLE education_types (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          type TEXT NOT NULL
        );

        CREATE TABLE education_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          education_type_id INTEGER NOT NULL,
          institution_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE NOT NULL,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id),
          FOREIGN KEY (education_type_id) REFERENCES education_types(id),
          FOREIGN KEY (institution_id) REFERENCES institutions(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id)
        );


        CREATE TABLE employers (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL
        );

        CREATE TABLE job_titles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          title TEXT NOT NULL
        );

        CREATE TABLE work_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          employer_id INTEGER NOT NULL,
          job_title_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE NOT NULL,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id),
          FOREIGN KEY (employer_id) REFERENCES employers(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id),
          FOREIGN KEY (job_title_id) REFERENCES job_titles(id)
          );

        CREATE TABLE skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL
        );

        CREATE TABLE colleague_skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          skill_id INTEGER NOT NULL,
          rating INTEGER,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id),
          FOREIGN KEY (skill_id) REFERENCES skills(id)
        );

        CREATE TABLE profiles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          title TEXT NOT NULL,          --similar to the file name
          education_history_ids TEXT,   --list of education history ids in specified order
          work_history_ids TEXT,        --list of work history ids in specified order
          colleague_skills_ids TEXT,    --list of skill ids in specified order
          ordering TEXT,                --The ordering of how the different sections will be (education, work, skills, etc)
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id)
        );

        ALTER TABLE colleagues ADD COLUMN main_profile_id INTEGER REFERENCES profiles(id);
        
        --Populate states table, since this likely will not change.
        INSERT INTO states (name, abbreviation)
        VALUES
          ('Alabama', 'AL'),
          ('Alaska', 'AK'),
          ('Arizona', 'AZ'),
          ('Arkansas', 'AR'),
          ('California', 'CA'),
          ('Colorado', 'CO'),
          ('Connecticut', 'CT'),
          ('Delaware', 'DE'),
          ('Florida', 'FL'),
          ('Georgia', 'GA'),
          ('Hawaii', 'HI'),
          ('Idaho', 'ID'),
          ('Illinois', 'IL'),
          ('Indiana', 'IN'),
          ('Iowa', 'IA'),
          ('Kansas', 'KS'),
          ('Kentucky', 'KY'),
          ('Louisiana', 'LA'),
          ('Maine', 'ME'),
          ('Maryland', 'MD'),
          ('Massachusetts', 'MA'),
          ('Michigan', 'MI'),
          ('Minnesota', 'MN'),
          ('Mississippi', 'MS'),
          ('Missouri', 'MO'),
          ('Montana', 'MT'),
          ('Nebraska', 'NE'),
          ('Nevada', 'NV'),
          ('New Hampshire', 'NH'),
          ('New Jersey', 'NJ'),
          ('New Mexico', 'NM'),
          ('New York', 'NY'),
          ('North Carolina', 'NC'),
          ('North Dakota', 'ND'),
          ('Ohio', 'OH'),
          ('Oklahoma', 'OK'),
          ('Oregon', 'OR'),
          ('Pennsylvania', 'PA'),
          ('Rhode Island', 'RI'),
          ('South Carolina', 'SC'),
          ('South Dakota', 'SD'),
          ('Tennessee', 'TN'),
          ('Texas', 'TX'),
          ('Utah', 'UT'),
          ('Vermont', 'VT'),
          ('Virginia', 'VA'),
          ('Washington', 'WA'),
          ('West Virginia', 'WV'),
          ('Wisconsin', 'WI'),
          ('Wyoming', 'WY');

        ";


        private static string? QueryGeneratorInsert(string tableName, params InputF[] fields)
        {
            if (fields.Length < 1)
                return null;

            /*
             * INSERT INTO 
                                work_history(colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description) 
                                VALUES (@colleague_id, @employer_id, @job_title_id, @municipality_id, @state_id, @start_date, @end_date, @description)
             */
            StringBuilder sb = new StringBuilder($"INSERT INTO {tableName} ("),
                columns = new StringBuilder($"{fields[0].ColumnName}"),
                values = new StringBuilder($"{fields[0].VariableName}");

            for (int i = 1; i < fields.Length - 1; i ++)
            {
                columns.Append($", {fields[i].ColumnName}");
                values.Append($", {fields[i].VariableName}");
            }

            sb.Append($"{columns.ToString()}) VALUES (");
            sb.Append($"{values.ToString()})");


            return sb.ToString();
        }

        
        protected class Field
        {
            public readonly string ColumnName;

            public readonly bool IsRequired;

            public object Value;

            public Field(string columnName, object value, bool isRequired)
            {
                this.ColumnName = columnName;
                this.Value = value;
                this.IsRequired = isRequired;
            }

            public string VariableName { get { return $"@{this.ColumnName}"; } }
        }
        public class InputF
        {
            public readonly string ColumnName;

            public object Value;

            public InputF(string columnName, object value)
            {
                this.ColumnName = columnName;
                this.Value = value;
            }

            public string VariableName { get { return $"@{this.ColumnName}"; } }
        }

        protected class Record
        {
            public readonly string TableName;

            public List<Field> Fields;

            public Record()
            {

            }
        }
        //*/
    }

}
