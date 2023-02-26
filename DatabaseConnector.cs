﻿using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Security.Principal;

namespace SCCPP1
{
    public class DatabaseConnector
    {

        private static string connStr = @"Data Source=CPPDatabse.db";

        //load these tables in memory to reduce CPU usage
        //may not need to do this
        private static Dictionary<int, string> skills = new Dictionary<int, string>();
        private static  Dictionary<int, string> education_types = new Dictionary<int, string>();
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
                            r.GetChars(3, 0, c, 0, 2);

                            //loads the ID as the key and the value as the string
                            table.TryAdd(r.GetInt32(1), c + "" + r.GetString(2));
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
                            table.TryAdd(r.GetInt32(1), r.GetString(2));
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

        #region Account Data
        /// <summary>
        /// Loads a new Account object into the SessionData provided, if the user exists.
        /// </summary>
        /// <param name="sessionData">The current session object</param>
        /// <returns>true if the account exists, false if the account does not exist</returns>
        public static bool LoadUserData(SessionData sessionData)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, email, name FROM account WHERE (user_hash=@user);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(sessionData.Username));
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return false;


                        //load new instance with basic colleague information
                        Account a = new Account(sessionData, true);

                        a.ID = r.GetInt32(1);
                        a.Role = r.GetInt32(2);
                        a.Name = r.GetString(3);
                        a.Email = r.GetString(4);

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
                string sql = @"INSERT INTO account (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id) VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id) RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@intro_narrative", introNarrative);
                    cmd.Parameters.AddWithValue("@main_profile_id", mainProfileID);

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
            /*   using (SqliteConnection conn = new SqliteConnection(connStr))
               {
                   conn.Open();
                   string sql = @"INSERT INTO account (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id) VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id) RETURNING id;";
                   using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                   {

                       cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(account.GetUsername()));

                       cmd.Parameters.AddWithValue("@role", account.Role);
                       cmd.Parameters.AddWithValue("@name", account.Name);
                       cmd.Parameters.AddWithValue("@email", account.Email);
                       cmd.Parameters.AddWithValue("@phone", account.Phone);
                       cmd.Parameters.AddWithValue("@address", account.Address);
                       cmd.Parameters.AddWithValue("@intro_narrative", account.IntroNarrative);
                       cmd.Parameters.AddWithValue("@main_profile_id", account.MainProfileID);

                       object? accountID = cmd.ExecuteScalar();

                       if (accountID == null)
                           return -1;

                       return account.ID = Convert.ToInt32(accountID);//return record ID

                   }
               }//*/
        }

        public static int UpdateUser(int id, string userID, int role, string name, string email, int phone, string address, string introNarrative, int mainProfileID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE account SET user_hash=@user_hash, role=@role, name=@name, email=@email, phone=@phone, address=@address, intro_narrative=@intro_narrative, main_profile_id=@main_profile_id WHERE id = @id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@intro_narrative", introNarrative);
                    cmd.Parameters.AddWithValue("@main_profile_id", mainProfileID);

                    //failed to insert or update account
                    /*if (cmd.ExecuteNonQuery() == 0)
                        return -1;

                    account.ID = GetAccountID(account.GetUsername());*/
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static int UpdateUser(Account account)
        {
            return UpdateUser(account.ID, account.GetUsername(), account.Role, account.Name, account.Email, account.Phone, account.Address, account.IntroNarrative, account.MainProfileID);
            /*using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE account SET user_hash=@user_hash, role=@role, name=@name, email=@email, phone=@phone, address=@address, intro_narrative=@intro_narrative, main_profile_id=@main_profile_id WHERE id = @id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(account.GetUsername()));

                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@name", account.Name);
                    cmd.Parameters.AddWithValue("@email", account.Email);
                    cmd.Parameters.AddWithValue("@phone", account.Phone);
                    cmd.Parameters.AddWithValue("@address", account.Address);
                    cmd.Parameters.AddWithValue("@intro_narrative", account.IntroNarrative);
                    cmd.Parameters.AddWithValue("@main_profile_id", account.MainProfileID);

                    //failed to insert or update account
                    if (cmd.ExecuteNonQuery() == 0)
                        return -1;

                    account.ID = GetAccountID(account.GetUsername());
                    return cmd.ExecuteNonQuery();
                }
            }//*/
        }

        public static int GetAccountID(string username)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, email, name FROM account WHERE (user_hash=@user);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(username));
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return -1;

                        return r.GetInt32(1);
                    }
                }
            }
        }



        //old code from my other db
        private static void printusers()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT *  FROM account;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Console.WriteLine($"'{r.GetString(1)}', '{r.GetString(2)}'");//user
                        }

                    }
                }
            }
        }


        public static Account? GetUser(string userID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM account WHERE (user_hash=@user_hash);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_hash", userID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return null;


                        //load new instance with basic colleague information
                        Account a = new Account(null, true);

                        a.ID = r.GetInt32(1);
                        a.Role = r.GetInt32(2);
                        a.Name = r.GetString(3);
                        a.Email = r.GetString(4);
                        a.Phone = r.GetInt32(5);
                        a.Address = r.GetString(6);
                        a.IntroNarrative = r.GetString(7);
                        a.MainProfileID = r.GetInt32(8);

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
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM account WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return null;


                        //load new instance with basic colleague information
                        Account a = new Account(null, true);

                        a.ID = r.GetInt32(1);
                        a.Role = r.GetInt32(2);
                        a.Name = r.GetString(3);
                        a.Email = r.GetString(4);
                        a.Phone = r.GetInt32(5);
                        a.Address = r.GetString(6);
                        a.IntroNarrative = r.GetString(7);
                        a.MainProfileID = r.GetInt32(8);

                        return a;
                    }
                }
            }
        }
        #endregion



        #region Skill Data
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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetString(1);

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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetInt32(1);

                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// Returns all of the saved colleague skills.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <returns>a list of strings that stores the colleague_skill_id\skill_id\rating in that format</returns>
        public static List<string>? GetColleagueSkills(Account account)
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
                    cmd.Parameters.AddWithValue("@id", account.ID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        string s;
                        while (r.Read())
                        {
                            s = r.GetInt32(1) + "\\"; //colleage skill id
                            s += r.GetInt32(3) + "\\"; //skill id
                            s += r.GetInt32(4) + ""; //rating for skill

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


        /*public static int InsertEducationHistory(EducationData ed)
        {
            return InsertEducationHistory()
        }*/

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


        public static int GetEducationTypeID(string educationType)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM education_types WHERE (type=@type);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@type", educationType);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetInt32(1);

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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetString(1);

                        return null;
                    }
                }
            }
        }

        public static string? GetInstitution(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT name FROM institutions WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetString(1);

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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetInt32(1);

                        return -1;
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

                        string s;
                        while (r.Read())
                        {
                            s = r.GetInt32(1) + "\\"; //record id
                            s += r.GetInt32(3) + "\\"; //education type id
                            s += r.GetInt32(4) + "\\"; //institution id
                            s += r.GetInt32(5) + "\\"; //municipality id
                            s += r.GetInt32(6) + "\\"; //state id

                            s += r.GetDateTime(7) + "\\"; //start date
                            s += r.GetDateTime(8) + "\\"; //end date (might be empty)

                            s += r.GetString(9) + "\\"; //description (might be empty)

                            list.Add(s);
                        }

                        return list;
                    }
                }
            }
        }


        public static List<EducationData>? GetColleagueEducationHistory(Account account)
        {
            if (account == null || account.ID < 0)
                return null;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.ID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<EducationData> list = new List<EducationData>();

                        while (r.Read())
                        {
                            EducationData ed = new EducationData(account, r.GetInt32(1));
                            ed.EducationTypeID = r.GetInt32(3);
                            ed.InstitutionID = r.GetInt32(4);
                            ed.Location = new Location(r.GetInt32(5), r.GetInt32(6));

                            ed.StartDate = Utilities.ToDateOnly(r.GetDateTime(7));
                            ed.EndDate = Utilities.ToDateOnly(r.GetDateTime(8));

                            ed.Description = r.GetString(9);

                            list.Add(ed);
                        }

                        return list;
                    }
                }
            }
        }

        #endregion


        #region Work Data

        public static int InsertWorkHistory(int colleagueID, int employerID, int municipalityID, int stateID, int jobTitleID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                work_history(colleague_id, employer_id, municipality_id, state_id, start_date, end_date, description) 
                                VALUES (@colleague_id, @employer_id, @municipality_id, @state_id, @start_date, @end_date, @description)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@employer_id", employerID);
                    cmd.Parameters.AddWithValue("@municipality_id", municipalityID);
                    cmd.Parameters.AddWithValue("@state_id", stateID);
                    cmd.Parameters.AddWithValue("@start_date", startDate);
                    cmd.Parameters.AddWithValue("@end_date", endDate);
                    cmd.Parameters.AddWithValue("@description", description);


                    object? workID = cmd.ExecuteScalar();

                    if (workID == null)
                        return -1;

                    return Convert.ToInt32(workID);//return record ID
                }
            }
        }


        public static int UpdateWorkHistory(int id, int colleagueID, int employerID, int municipalityID, int stateID, int jobTitleID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE work_history SET 
                                colleague_id=@colleague_id, employer_id=@employer_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description WHERE id = @id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    cmd.Parameters.AddWithValue("@employer_id", employerID);
                    cmd.Parameters.AddWithValue("@municipality_id", municipalityID);
                    cmd.Parameters.AddWithValue("@state_id", stateID);
                    cmd.Parameters.AddWithValue("@start_date", startDate);
                    cmd.Parameters.AddWithValue("@end_date", endDate);
                    cmd.Parameters.AddWithValue("@description", description);

                    return cmd.ExecuteNonQuery();//rows affected
                }
            }
        }


        public static int GetEmployerID(string employer)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id FROM employers WHERE (name=@name);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", employer);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetInt32(1);

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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetString(1);

                        return null;
                    }
                }
            }
        }

        public static string? GetJobTitle(int id)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT title FROM job_titles WHERE (id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetString(1);

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
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return r.GetInt32(1);

                        return -1;
                    }
                }
            }
        }


        public static List<string> GetRawColleagueWorkHistory(int colleagueID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, employer_id, municipality_id, state_id, start_date, end_date, description FROM work_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", colleagueID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        string s;
                        while (r.Read())
                        {
                            s = r.GetInt32(1) + "\\"; //record id
                            s += r.GetInt32(3) + "\\"; //employer id
                            s += r.GetInt32(4) + "\\"; //municipality id
                            s += r.GetInt32(5) + "\\"; //state id

                            s += r.GetDateTime(6) + "\\"; //start date
                            s += r.GetDateTime(7) + "\\"; //end date (might be empty)

                            s += r.GetString(8) + "\\"; //description (might be empty)

                            list.Add(s);
                        }

                        return list;
                    }
                }
            }
        }


        public static List<WorkData>? GetColleagueWorkHistory(Account account)
        {
            if (account == null || account.ID < 0)
                return null;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, colleague_id, employer_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.ID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<WorkData> list = new List<WorkData>();

                        while (r.Read())
                        {
                            WorkData wd = new WorkData(account, r.GetInt32(1));
                            wd.EmployerID = r.GetInt32(3);
                            wd.Location = new Location(r.GetInt32(4), r.GetInt32(5));

                            wd.StartDate = Utilities.ToDateOnly(r.GetDateTime(6));
                            wd.EndDate = Utilities.ToDateOnly(r.GetDateTime(7));

                            wd.Description = r.GetString(8);

                            list.Add(wd);
                        }

                        return list;
                    }
                }
            }
        }
        #endregion

        /*
        public static bool LoadUserData(SessionData sessionData)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, email, name FROM account WHERE (user_hash=@user);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(sessionData.Username));
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return false;


                        //load new account with basic colleague information
                        Account a = new Account(sessionData);

                        a.ID = r.GetInt32(1);
                        a.Role = r.GetInt32(2);
                        a.Name = r.GetString(3);
                        a.Email = r.GetString(4);

                        sessionData.Account = a;

                        return true;
                    }
                }
            }
        }
        //*/




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
          municipality_id INTEGER,
          state_id INTEGER,
          job_title_id INTEGER NOT NULL,
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
          FOREIGN KEY (colleague_id) REFERENCES colleagues(colleague_id),
          FOREIGN KEY (skill_id) REFERENCES skills(skill_id)
        );

        CREATE TABLE profiles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          title TEXT NOT NULL,          --similar to the file name
          education_history_ids TEXT,   --list of education history ids in specified order
          work_history_ids TEXT,        --list of work history ids in specified order
          colleague_skills_ids TEXT,    --list of skill ids in specified order
          ordering TEXT,                --The ordering of how the different sections will be (education, work, skills, etc)
          FOREIGN KEY (colleague_id) REFERENCES colleagues(colleague_id)
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


        protected class Row<TValue>
        {
            public readonly string ColumnName;

            public readonly TValue Value;

            public Row(string columnName, TValue value)
            {
                this.ColumnName = columnName;
                this.Value = value;
            }
        }
    }

}
