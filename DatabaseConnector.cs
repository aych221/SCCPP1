using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;
using System.Text;
using SCCPP1.Database.Tables;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using SCCPP1.Database;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;

namespace SCCPP1
{
    public class DatabaseConnector
    {

        private static string connStr = @"Data Source=CPPDatabse.db";

        //load these tables in memory to reduce CPU usage
        //may not need to do this
        private static Dictionary<int, string> skills;
        private static Dictionary<int, string> education_types;
        private static Dictionary<int, string> institutions;
        private static Dictionary<int, string> municipalities;

        //states are saved as abbreviation on first two chars and the other chars are the full name
        private static Dictionary<int, string> states;
        private static Dictionary<int, string> employers;
        private static Dictionary<int, string> job_titles;

        public static TableModels TableModels;

        public static void CreateDatabase(bool loadCaches = false)
        {

            TableModels = new TableModels();
            Dictionary<string, DbTable> Tables = TableModels.Tables;

            StringBuilder sql = new StringBuilder();

            //drop tables if exists
            sql.AppendLine(QueryGenerator.DropTables(Tables));

            //generate sql for tables
            foreach (DbTable table in Tables.Values)
                sql.AppendLine(QueryGenerator.CreateTableSql(table));

            //add foreign key to colleagues table
            sql.AppendLine(QueryGenerator.AlterTableAddForeignKeys(Tables["colleagues"], new Field("main_profile_id", typeof(int), false, false, Tables["profiles"].PrimaryKey)));

            /*            DbRecord[] states = new DbRecord[]
                        {
                            new DbRecord(new DbStateData("Alabama", "AL")),
                            new DbRecord(new DbStateData("Alaska", "AK")),
                            new DbRecord(new DbStateData("Arizona", "AZ")),
                            new DbRecord(new DbStateData("Arkansas", "AR")),
                            new DbRecord(new DbStateData("California", "CA")),
                            new DbRecord(new DbStateData("Colorado", "CO")),
                            new DbRecord(new DbStateData("Connecticut", "CT")),
                            new DbRecord(new DbStateData("Delaware", "DE")),
                            new DbRecord(new DbStateData("Florida", "FL")),
                            new DbRecord(new DbStateData("Georgia", "GA")),
                            new DbRecord(new DbStateData("Hawaii", "HI")),
                            new DbRecord(new DbStateData("Idaho", "ID")),
                            new DbRecord(new DbStateData("Illinois", "IL")),
                            new DbRecord(new DbStateData("Indiana", "IN")),
                            new DbRecord(new DbStateData("Iowa", "IA")),
                            new DbRecord(new DbStateData("Kansas", "KS")),
                            new DbRecord(new DbStateData("Kentucky", "KY")),
                            new DbRecord(new DbStateData("Louisiana", "LA")),
                            new DbRecord(new DbStateData("Maine", "ME")),
                            new DbRecord(new DbStateData("Maryland", "MD")),
                            new DbRecord(new DbStateData("Massachusetts", "MA")),
                            new DbRecord(new DbStateData("Michigan", "MI")),
                            new DbRecord(new DbStateData("Minnesota", "MN")),
                            new DbRecord(new DbStateData("Mississippi", "MS")),
                            new DbRecord(new DbStateData("Missouri", "MO")),
                            new DbRecord(new DbStateData("Montana", "MT")),
                            new DbRecord(new DbStateData("Nebraska", "NE")),
                            new DbRecord(new DbStateData("Nevada", "NV")),
                            new DbRecord(new DbStateData("New Hampshire", "NH")),
                            new DbRecord(new DbStateData("New Jersey", "NJ")),
                            new DbRecord(new DbStateData("New Mexico", "NM")),
                            new DbRecord(new DbStateData("New York", "NY")),
                            new DbRecord(new DbStateData("North Carolina", "NC")),
                            new DbRecord(new DbStateData("North Dakota", "ND")),
                            new DbRecord(new DbStateData("Ohio", "OH")),
                            new DbRecord(new DbStateData("Oklahoma", "OK")),
                            new DbRecord(new DbStateData("Oregon", "OR")),
                            new DbRecord(new DbStateData("Pennsylvania", "PA")),
                            new DbRecord(new DbStateData("Rhode Island", "RI")),
                            new DbRecord(new DbStateData("South Carolina", "SC")),
                            new DbRecord(new DbStateData("South Dakota", "SD")),
                            new DbRecord(new DbStateData("Tennessee", "TN")),
                            new DbRecord(new DbStateData("Texas", "TX")),
                            new DbRecord(new DbStateData("Utah", "UT")),
                            new DbRecord(new DbStateData("Vermont", "VT")),
                            new DbRecord(new DbStateData("Virginia", "VA")),
                            new DbRecord(new DbStateData("Washington", "WA")),
                            new DbRecord(new DbStateData("West Virginia", "WV")),
                            new DbRecord(new DbStateData("Wisconsin", "WI")),
                            new DbRecord(new DbStateData("Wyoming", "WY"))
                        };*/

            Account t = new Account("testuser");
            t.UpdateData("Testing", "Some", "User", "test@user.edu", 1231231234, "Nothing interesting");
            //Console.WriteLine(QueryGenerator.InsertOrIgnore(Tables["colleagues"], new DbRecord(new DbColleagueData(t))));
            Console.WriteLine();
            Console.WriteLine(new DbQueryBuilder().SelectAll(Tables["work_histories"].Columns[1]));
            Console.WriteLine();
            Console.WriteLine(QueryGenerator.UpdateAll(Tables["colleagues"]));
            Console.WriteLine();
            Console.WriteLine(QueryGenerator.UpdateRequiredOnly(Tables["colleagues"]));

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                Console.WriteLine("Server Version: " + conn.ServerVersion);
                using (SqliteCommand cmd = new SqliteCommand(dbSQL, conn))
                {
                    cmd.ExecuteNonQuery();

                    //QueryGenerator.InsertOrIgnore(cmd, TableModels.States, states);

                    if (loadCaches)
                        LoadCaches();
                    else
                    {
                        LoadCacheStates();
                    }

                }
            }

        }


        #region Dictionary Loaders
        //Dictionary loaders
        private static void LoadCacheSkills()
        {
            skills = LoadTwoColumnTable("skills");
        }

        private static void LoadCacheEducationTypes()
        {
            education_types = LoadTwoColumnTable("education_types");
        }
        private static void LoadCacheInstitutions()
        {
            institutions = LoadTwoColumnTable("institutions");
        }

        private static void LoadCacheMunicipalities()
        {
            municipalities = LoadTwoColumnTable("municipalities");
        }

        //maybe don't need, could just hard code
        private static void LoadCacheStates()
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

        private static void LoadCacheEmployers()
        {
            employers = LoadTwoColumnTable("employers");
        }

        private static void LoadCacheJobTitles()
        {
            job_titles = LoadTwoColumnTable("job_titles");
        }

        private static void LoadCaches()
        {
            LoadCacheSkills();
            LoadCacheInstitutions();
            LoadCacheEducationTypes();
            LoadCacheMunicipalities();
            LoadCacheStates();
            LoadCacheEmployers();
            LoadCacheJobTitles();
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
        private static string? GetCachedSkill(int id)
        {
            return GetCachedValue(skills, id);
        }

        private static string[] GetCachedSkills(params int[] ids)
        {
            return GetCachedValues(skills, ids);
        }


        private static string? GetCachedEducationType(int id)
        {
            return GetCachedValue(education_types, id);
        }

        private static string[] GetCachedEducationTypes(params int[] ids)
        {
            return GetCachedValues(education_types, ids);
        }


        private static string? GetCachedInstitution(int id)
        {
            return GetCachedValue(institutions, id);
        }

        private static string[] GetCachedInstitutions(params int[] ids)
        {
            return GetCachedValues(institutions, ids);
        }


        public static string? GetCachedMunicipality(int id)
        {
            return GetCachedValue(municipalities, id);
        }

        public static string[] GetCachedMunicipalities(params int[] ids)
        {
            return GetCachedValues(municipalities, ids);
        }


        public static string? GetCachedState(int id)
        {
            return GetCachedValue(states, id);
        }

        public static string[] GetCachedStates(params int[] ids)
        {
            return GetCachedValues(states, ids);
        }


        private static string? GetCachedEmployer(int id)
        {
            return GetCachedValue(employers, id);
        }

        private static string[] GetCachedEmployers(params int[] ids)
        {
            return GetCachedValues(employers, ids);
        }


        private static string? GetCachedJobTitle(int id)
        {
            return GetCachedValue(job_titles, id);
        }

        private static string[] GetCachedJobTitles(params int[] ids)
        {
            return GetCachedValues(job_titles, ids);
        }



        private static string? GetCachedValue(Dictionary<int, string> table, int id)
        {
            string? s;

            if (table.TryGetValue(id, out s))
                return s;

            return null;
        }

        private static string[] GetCachedValues(Dictionary<int, string> table, params int[] ids)
        {
            string[] arr = new string[ids.Length];
            string s;

            for (int i = 0; i < arr.Length; i++)

                //value was found
                if (table.TryGetValue(ids[i], out s))
                    arr[i] = s;

            return arr;
        }




        //first searches the dictionary for the skill,
        //if not found, it will search the database
        //if nothing is found, it will return null.
        private static string? TryGetCachedSkill(int id)
        {
            //check cache, if not there, reload cache
            if (GetCachedSkill(id) == null)
                LoadCacheSkills();

            return GetCachedSkill(id);
        }

        //may not use
        private static string[] TryGetCachedSkills(params int[] ids)
        {
            return GetCachedSkills(ids);
        }


        #endregion

        #region User Data
        /// <summary>
        /// Loads a new Account object into the SessionData provided, if the user exists.
        /// </summary>
        /// <param name="sessionData">The current session object</param>
        /// <returns>true if the account exists, false if the account does not exist</returns>

        [Obsolete("Use GetAccount() instead.")]
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
                        a.EmailAddress = GetString(r, 3);

                        sessionData.Owner = a;

                        return true;
                    }
                }
            }
        }


        public static int InsertUser(string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO colleagues (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id) VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id) RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(email));
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



        public static bool InsertAccount(Account account)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO colleagues (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id)
                                VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    //since this is hashed, we won't need to clean the value
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(account.GetUsername()));

                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(account.Name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(account.EmailAddress));
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(account.PhoneNumber));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(account.StreetAddress));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(account.IntroNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(account.MainProfileID));

                    object? accountID = cmd.ExecuteScalar();

                    if (accountID == null)
                        return false;


                    return (account.RecordID = Convert.ToInt32(accountID)) > 0;

                }
            }
        }


        public static int UpdateUser(Account account)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE colleagues
                                SET user_hash=@user_hash, role=@role, name=@name, email=@email, phone=@phone, address=@address, intro_narrative=@intro_narrative, main_profile_id=@main_profile_id
                                WHERE id = @id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@id", account.RecordID);
                    AddParameterValues(account, cmd.Parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Updates all the <see cref="SqliteCommand.Parameters"></see> in the command with the values from the account object.
        /// This does not update the ID.
        /// </summary>
        /// <param name="account">The account to pull values from.</param>
        /// <param name="parameters">The collection of parameters from the command.</param>
        private static void AddParameterValues(Account account, SqliteParameterCollection parameters)
        {
            //since this is hashed, we won't need to clean the value
            parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(account.GetUsername()));
            
            parameters.AddWithValue("@role", account.Role);
            parameters.AddWithValue("@name", ValueCleaner(account.Name));
            parameters.AddWithValue("@email", ValueCleaner(account.EmailAddress));
            parameters.AddWithValue("@phone", ValueCleaner(account.PhoneNumber));
            parameters.AddWithValue("@address", ValueCleaner(account.StreetAddress));
            parameters.AddWithValue("@intro_narrative", ValueCleaner(account.IntroNarrative));
            parameters.AddWithValue("@main_profile_id", ValueCleaner(account.MainProfileID));
        }

        public static int UpdateUser(int id, string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
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
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(email));
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(phone));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(address));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(introNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(mainProfileID));

                    return cmd.ExecuteNonQuery();
                }
            }
        }


        //used to save an account if you already have an id.
        //just to be save, it still checks to see if the user exists.
        //put -1 if id is unknown
        public static bool SaveUser(int id, string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
        {
            if (!ExistsUser(id))
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
        }

        //used if you don't have an ID
        //put -1 if id is unknown
        public static int SaveUser(string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
        {
            int id = ExistsUser(userID);
            if (id < 1)
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID);
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID);
        }

        //used to save an account object, this will determine if the user needs to be created or not
        public static bool SaveUser(Account account)
        {
            if (account.IsReturning)
                return SaveUser(account.RecordID, account.GetUsername(), account.Role, account.Name, account.EmailAddress, account.PhoneNumber, account.StreetAddress, account.IntroNarrative, account.MainProfileID);
            account.RecordID = SaveUser(account.GetUsername(), account.Role, account.Name, account.EmailAddress, account.PhoneNumber, account.StreetAddress, account.IntroNarrative, account.MainProfileID);
            account.IsReturning = true;
            return account.RecordID >= 0;
        }


        public static bool ExistsUser(int id)
        {
            if (id < 1)
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



        /// <summary>
        /// Loads a new Account object into the SessionData provided.
        /// If the account does not exist, it will create a new account, but will not save it to the database.
        /// </summary>
        /// <param name="data">The current SessionData object</param>
        /// <returns>A new <see cref="Account"/> instance.</returns>
        public static Account GetAccount(SessionData data)
        {
            Account account;

            if ((account = LoadAccount(data)) == null)
                account = new Account(data, false); //may want to use Create user, but not sure if we want to save account to db if they don't save on CreateMainProfile

            return account;
        }


        /**
         * 

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

         * 
         * 
         * 
         */
        public static int CreateUser(SessionData data)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO colleagues (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id) VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id) RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    Account account = new Account(data, false);

                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(account.GetUsername()));

                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(account.Name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(account.EmailAddress));

                    object? accountID = cmd.ExecuteScalar();

                    if (accountID == null)
                        return -1;

                    return account.RecordID = Convert.ToInt32(accountID);//return record ID

                }
            }
        }


        public static Account LoadAccount(SessionData data)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM colleagues WHERE (user_hash=@user_hash);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_hash", data.Username);
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        //account was found.
                        if (!r.Read())
                            return null;

                        //load new instance with basic colleague information
                        Account account = new Account(data, true);

                        account.RecordID = GetInt32(r, 0);
                        account.Role = GetInt32(r, 1);
                        account.Name = GetString(r, 2);
                        account.EmailAddress = GetString(r, 3);
                        account.PhoneNumber = GetInt64(r, 4);
                        account.StreetAddress = GetString(r, 5);
                        account.IntroNarrative = GetString(r, 6);
                        account.MainProfileID = GetInt32(r, 7);

                        return account;

                    }
                }
            }
        }



        [Obsolete ("Use GetAccount() instead.")]
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
                        a.EmailAddress = GetString(r, 3);
                        a.PhoneNumber = GetInt64(r, 4);
                        a.StreetAddress = GetString(r, 5);
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
                        a.EmailAddress = GetString(r, 4);
                        a.PhoneNumber = GetInt64(r, 5);
                        a.StreetAddress = GetString(r, 6);
                        a.IntroNarrative = GetString(r, 7);
                        a.MainProfileID = GetInt32(r, 8);

                        return a;
                    }
                }
            }
        }

        [Obsolete("Use GetAccount() instead.")]
        public static bool GetUser(Account acc)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id FROM colleagues WHERE (user_hash=@user_hash);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_hash", acc.GetUsername());
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        //could not find account.
                        //redirect account to creation page, if any input is entered, save new account to database
                        if (!r.Read())
                            return false;


                        //load new instance with basic colleague information
                        acc.IsReturning = true;
                        acc.RecordID = GetInt32(r, 0);
                        acc.Role = GetInt32(r, 1);
                        acc.Name = GetString(r, 2);
                        acc.EmailAddress = GetString(r, 3);
                        acc.PhoneNumber = GetInt64(r, 4);
                        acc.StreetAddress = GetString(r, 5);
                        acc.IntroNarrative = GetString(r, 6);
                        acc.MainProfileID = GetInt32(r, 7);

                        return true;
                    }
                }
            }
        }
        #endregion


        #region Skill Data

        //TODO, do we need to care about case?
        private static int SaveSkill(SkillData skill)
        {
            return SaveSkill(skill.SkillName);
        }

        //TODO, do we need to care about case?
        private static int SaveSkill(string skillName)
        {
            int id = GetSkillID(skillName);

            if (id == -1)
                return InsertSkill(skillName);

            return id;
        }

        private static int[] SaveSkills(params SkillData[] skills)
        {
            string[] skillNames = new string[skills.Length];
            for (int i = 0; i < skillNames.Length; i++)
                skillNames[i] = skills[i].SkillName;
            return SaveSkills(skillNames);
        }

        private static int[] SaveSkills(params string[] skillNames)
        {
            return InsertSkills(skillNames);
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


        /// <summary>
        /// Inserts skill names into the skills table. This will insert all unique skill names.
        /// </summary>
        /// <param name="skillNames">the skills to be inserted</param>
        /// <returns>GetSkillIDs(skillNames)</returns>
        public static int[] InsertSkills(params string[] skillNames)
        {
            if (skillNames.Length < 1)
                return null;


            StringBuilder sb = new StringBuilder("INSERT OR IGNORE INTO skills(name) VALUES (@skillName0)");

            for (int i = 1; i < skillNames.Length; i++)
            {
                sb.Append(',');
                sb.Append($"(@skillName{i})");
            }
            sb.Append(';');
            string sql = sb.ToString();

            //Console.WriteLine(sql);//skillsList.ToString());

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    for (int i = 0; i < skillNames.Length; i++)
                        cmd.Parameters.AddWithValue($"@skillName{i}", ValueCleaner(skillNames[i]));

                    //Console.WriteLine("Rows effected: " + cmd.ExecuteNonQuery());
                }
            }

            return GetSkillIDs(skillNames);

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
        /// Retrieves the skill name(s) based on the ids.
        /// </summary>
        /// <param name="id">the skill id</param>
        /// <returns>null if skill id does not exist, otherwise returns skill name</returns>
        public static string[] GetSkillNames(params int[] ids)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                //not much concern with SQL injection since only ints are being inserted.
                string sql = $"SELECT name FROM skills WHERE id IN ({string.Join(",", ids)});";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        List<string> skillNames = new List<string>();
                        while (r.Read())
                            skillNames.Add(GetString(r, 0));

                        return skillNames.ToArray();
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
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(skillName));
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
        public static int[]? GetSkillIDs(params string[] skillNames)
        {
            if (skillNames.Length < 1)
                return null;

            Dictionary<string, int> skillNameResults = new Dictionary<string, int>(skillNames.Length);
            int[] skillIDs = new int[skillNames.Length];

            //create skills list
            StringBuilder sb = new StringBuilder("SELECT name, id FROM skills WHERE name IN(@skillName0");
            skillNameResults.Add(skillNames[0], -1);


            for (int i = 1; i < skillNames.Length; i++)
            {
                sb.Append(',');
                sb.Append($"@skillName{i}");

                //fill with -1's initially to indicate not found
                skillNameResults.Add(skillNames[i], -1);
                skillIDs[i] = -1;
            }
            sb.Append(");");

            string sql = sb.ToString();

            //execute select query
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    for (int i = 0; i < skillNames.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@skillName{i}", ValueCleaner(skillNames[i]));
                    }
                    //Console.WriteLine(cmd.CommandText);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        //put results in dictionary
                        while (r.Read())
                            skillNameResults[GetString(r, 0)] = GetInt32(r, 1);


                        //refresh skillIds with dictionary data
                        for (int i = 0; i < skillNames.Length; i++)
                            skillIDs[i] = skillNameResults[skillNames[i]];

                        //Console.Write("SkillIDs: ");
                        //Console.WriteLine(string.Join(",", skillIDs));

                        return skillIDs;
                    }
                }
            }
        }

        public static bool SaveColleageSkills(Account account)
        {
            //toInsert are skills with -1 ids, toUpdate are skills with existing ids
            List<string> toInsert = new List<string>(), updateCases = new List<string>();

            //ids to update
            List<int> updateIDs = new List<int>(), updateRatings = new List<int>();

            List<SkillData> skills = account.Skills;

            //for SaveSkills method
            string[] skillNames = new string[skills.Count];

            for (int i = 0; i < skills.Count; i++)
            {
                skillNames[i] = skills[i].SkillName;

                //toUpdate
                if (skills[i].RecordID > 0)
                {
                    updateIDs.Add(skills[i].RecordID);
                    updateRatings.Add(skills[i].Rating);
                    updateCases.Add($"WHEN {skills[i].RecordID} THEN @rating{i}");
                }
                else
                {
                    toInsert.Add($"({account.RecordID},@skillID{i})");
                }
            }


            //insert skills and get ids
            int[] ids = SaveSkills(skillNames);


            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //not sure if this is the most efficient solution for this table
                //will need to run tests
                string sql;

                //inserts first
                if (toInsert.Count > 0)
                {
                    sql = $"INSERT OR IGNORE INTO colleague_skills(colleague_id, skill_id) VALUES {ValueCleaner(string.Join(",", toInsert))};";
                    using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                    {
                        for (int i = 0; i < ids.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@skillID{i}", ids[i]);
                        }
                        cmd.ExecuteNonQuery();

                    }
                }

                if (updateIDs.Count > 0)
                {
                    //now do updates
                    sql = $@"UPDATE colleague_skills
                            SET rating = CASE id {ValueCleaner(string.Join(Environment.NewLine, updateCases))} END
                            WHERE id IN ({ValueCleaner(string.Join(",", updateIDs))});";

                    using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                    {
                        for (int i = 0; i < ids.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@rating{i}", updateRatings[i]);
                        }
                        cmd.ExecuteNonQuery();

                    }
                }

                return true;
            }

            return false;
        }

        public static bool SaveColleageSkills(params SkillData[] skills)
        {
            //toInsert are skills with -1 ids, toUpdate are skills with existing ids
            List<string> toInsert = new List<string>(), updateCases = new List<string>();

            //ids to update
            List<int> updateIDs = new List<int>(), updateRatings = new List<int>();


            //for SaveSkills method
            string[] skillNames = new string[skills.Length];

            for (int i = 0; i < skills.Length; i++)
            {
                skillNames[i] = skills[i].SkillName;

                //toUpdate
                if (skills[i].RecordID > 0)
                {
                    updateIDs.Add(skills[i].RecordID);
                    updateRatings.Add(skills[i].Rating);
                    updateCases.Add($"WHEN {skills[i].RecordID} THEN @rating{i}");
                }
                else
                {
                    toInsert.Add($"({skills[i].Owner.RecordID},@skillID{i})");
                }
            }


            //insert skills and get ids
            int[] ids = SaveSkills(skillNames);


            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                //not sure if this is the most efficient solution for this table
                //will need to run tests
                string sql;

                //inserts first
                if (toInsert.Count > 0)
                {
                    sql = $"INSERT OR IGNORE INTO colleague_skills(colleague_id, skill_id) VALUES {ValueCleaner(string.Join(",", toInsert))};";
                    using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                    {
                        for (int i = 0; i < ids.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@skillID{i}", ids[i]);
                        }
                        //Console.WriteLine(cmd.CommandText);
                        cmd.ExecuteNonQuery();

                    }
                }

                if (updateIDs.Count > 0)
                {
                    //now do updates
                    sql = $@"UPDATE colleague_skills
                            SET rating = CASE id {ValueCleaner(string.Join(Environment.NewLine, updateCases))} END
                            WHERE id IN ({ValueCleaner(string.Join(",", updateIDs))});";

                    using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                    {
                        for (int i = 0; i < ids.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@rating{i}", updateRatings[i]);
                        }
                        //Console.WriteLine(cmd.CommandText);
                        cmd.ExecuteNonQuery();

                    }
                }

                return true;
            }

            return false;
        }



        /*
         * 
         * 
        CREATE TABLE colleague_skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          skill_id INTEGER NOT NULL,
          skill_category_id INTEGER,
          rating INTEGER,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id),
          FOREIGN KEY (skill_id) REFERENCES skills(id),
          FOREIGN KEY (skill_category_id) REFERENCES skill_categories(id)
        );
         */
        public static bool SaveColleagueSkill(SkillData sd)
        {

            if (sd.Remove)
            {
                sd.IsRemoved = DeleteRecord("colleague_skills", sd.RecordID);
                Console.WriteLine("Removed Skill " + sd.SkillName + "? " + sd.IsRemoved);
                return sd.IsRemoved;
            }

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                using (SqliteCommand cmd = new SqliteCommand("", conn))
                {
                    //insert or get skill and skill category ids
                    sd.SkillID = SaveSkill(sd.SkillName);

                    //check if null (since category is not required to be set)
                    if (sd.SkillCategoryName != null)
                        sd.SkillCategoryID = InsertOrIgnore(cmd, "skill_categories", "name", sd.SkillCategoryName);

                    if (sd.RecordID > 0)
                    {
                        cmd.CommandText = @"UPDATE colleague_skills
                                            SET skill_id=@skill_id, skill_category_id=@skill_category_id, rating=@rating
                                            WHERE id=@id;";

                        cmd.Parameters.AddWithValue("@id", sd.RecordID);
                        cmd.Parameters.AddWithValue("@skill_id", sd.SkillID);
                        cmd.Parameters.AddWithValue("@skill_category_id", ValueCleaner(sd.SkillCategoryID));
                        cmd.Parameters.AddWithValue("@rating", ValueCleaner(sd.Rating));

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {


                        cmd.CommandText = @"INSERT INTO colleague_skills(colleague_id, skill_id, skill_category_id, rating)
                                            VALUES (@colleagueID, @skill_id, @skill_category_id, @rating)
                                            RETURNING id;";

                        cmd.Parameters.AddWithValue("@colleagueID", sd.Owner.RecordID);
                        cmd.Parameters.AddWithValue("@skill_id", sd.SkillID);
                        cmd.Parameters.AddWithValue("@skill_category_id", ValueCleaner(sd.SkillCategoryID));
                        cmd.Parameters.AddWithValue("@rating", ValueCleaner(sd.Rating));


                        object? id = cmd.ExecuteScalar();

                        if (id == null)
                            return false;

                        sd.RecordID = Convert.ToInt32(id);
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Returns all of the saved colleague skills.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <returns>a list of SkillData that stores the record in memory</returns>
        public static List<SkillData>? GetColleagueSkills(Account account)
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
                        List<SkillData> list = new List<SkillData>();

                        SkillData sd;
                        while (r.Read())
                        {

                            sd = new SkillData(account, GetInt32(r, 0));
                            sd.SkillID = GetInt32(r, 2);
                            sd.Rating = GetInt32(r, 3);

                            list.Add(sd);
                        }

                        return list;
                    }
                }
            }
        }


        /// <summary>
        /// Returns all of the saved colleague skills.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <param name="skillIds">array of skill ids in order of loading</param>
        /// <returns>a list of SkillData that stores the record in memory</returns>
        public static List<SkillData>? GetColleagueSkills(Account account, out List<int>? skillIds)
        {
            //session must've not had an account, so user must not exist
            if (account == null)
            {
                skillIds = null;
                return null;
            }

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT * FROM colleague_skills WHERE (colleague_id=@id);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", account.RecordID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        List<SkillData> list = new List<SkillData>();

                        //this is used so that we don't need to open and close multiple sessions
                        //and so we don't have to run a loop more than once when populating skillNames
                        skillIds = new List<int>();

                        SkillData sd;

                        while (r.Read())
                        {

                            sd = new SkillData(account, GetInt32(r, 0));
                            skillIds.Add(sd.SkillID = GetInt32(r, 2));
                            sd.Rating = GetInt32(r, 3);

                            list.Add(sd);
                        }

                        return list;
                    }
                }
            }
        }


        /// <summary>
        /// Loads the colleague's skills into the Account class. This will populate the Account.Skills list.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <param name="useCache">optional param</param>
        /// <returns>true if skills could be loaded, false if not</returns>
        public static bool LoadColleagueSkills1(Account account, bool useCache = false)
        {
            if (account == null || account.RecordID < 0)
                return false;


            List<SkillData> list;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                string sql = @"SELECT cs.id, cs.colleague_id, sc.name AS skill_category, cs.skill_category_id, s.name AS skill, cs.skill_id, cs.rating
                                            FROM colleague_skills cs
                                            JOIN skills s ON cs.skill_id = s.id
                                            LEFT JOIN skill_categories sc ON cs.skill_category_id = sc.id
                                            WHERE cs.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<SkillData>();

                        SkillData sd;
                        while (r.Read())
                        {
                            //SkillData(Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating)
                            sd = new SkillData(account, GetInt32(r, 0), GetString(r, 2), GetInt32(r, 3), GetString(r, 4), GetInt32(r, 5), GetInt32(r, 6));
                            Console.WriteLine($"Skill: {sd.SkillName} - {sd.SkillID} - {sd.SkillCategoryName} - {sd.SkillCategoryID} - {sd.Rating}");
                            list.Add(sd);
                        }

                        account.Skills = list;
                    }
                }
            }

            //Console.WriteLine($"Found Colleage_Skill Records: {list?.Count}");

            return true;
        }



        /// <summary>
        /// Loads the colleague's skills into the Account class. This will populate the Account.Skills list.
        /// </summary>
        /// <param name="account">The account associated with the skills</param>
        /// <param name="useCache">optional param</param>
        /// <returns>true if skills could be loaded, false if not</returns>
        public static bool LoadColleagueSkills1(Account account, out Dictionary<int, SkillData> dict, bool useCache = false)
        {
            dict = new();


            if (account == null || account.RecordID < 0)
                return false;


            List<SkillData> list;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                string sql = @"SELECT cs.id, cs.colleague_id, sc.name AS skill_category, cs.skill_category_id, s.name AS skill, cs.skill_id, cs.rating
                                            FROM colleague_skills cs
                                            JOIN skills s ON cs.skill_id = s.id
                                            LEFT JOIN skill_categories sc ON cs.skill_category_id = sc.id
                                            WHERE cs.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<SkillData>();

                        SkillData sd;
                        while (r.Read())
                        {
                            //SkillData(Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating)
                            sd = new SkillData(account, GetInt32(r, 0), GetString(r, 2), GetInt32(r, 3), GetString(r, 4), GetInt32(r, 5), GetInt32(r, 6));

                            //Console.WriteLine($"Skill: {sd.SkillName} - {sd.SkillID} - {sd.SkillCategoryName} - {sd.SkillCategoryID} - {sd.Rating}");
                            list.Add(sd);
                            dict.TryAdd(sd.RecordID, sd);
                        }

                        account.Skills = list;
                    }
                }
            }

            //Console.WriteLine($"Found Colleage_Skill Records: {dict?.Count}");

            return true;
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
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(municipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(stateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(startDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(endDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(description));

                    //Console.WriteLine($"Inserting education_history for {colleagueID} edutypeid: {educationTypeID}, instid: {institutionID}...");
                    object? educationID = cmd.ExecuteScalar();

                    if (educationID == null)
                        return -1;

                    //Console.WriteLine($"Inserted education_history for {colleagueID}, recordID: {Convert.ToInt32(educationID)}");

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
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(municipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(stateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(startDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(endDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(description));
                    return cmd.ExecuteNonQuery();//rows affected
                }
            }
        }

        public static int UpdateEducationHistory(EducationData ed)
        {
            return UpdateEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationTypeID, ed.InstitutionID, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }

        public static bool SaveEducationHistory(EducationData ed)
        {

            if (ed.Remove)
            {
                return ed.IsRemoved = DeleteRecord("education_history", ed.RecordID);
            }

            ed.EducationTypeID = SaveEducationType(ed.EducationType);
            ed.InstitutionID = SaveInstitution(ed.Institution);

            //Thread.Sleep(1);
            if (ExistsEducationHistory(ed.RecordID))
                return UpdateEducationHistory(ed) >= 0;
            return (ed.RecordID = InsertEducationHistory(ed)) >= 0;
            //return SaveEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationType, ed.Institution, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
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

                    //Console.WriteLine($"Inserted education_type: {type}");

                    object? educationTypeID = cmd.ExecuteScalar();

                    if (educationTypeID == null)
                        return -1;
                    //Console.WriteLine($"Inserted education_type: id:{Convert.ToInt32(educationTypeID)}, {type}");

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
                    //Console.WriteLine($"Inserting institution: {name}");


                    object? institutionID = cmd.ExecuteScalar();

                    if (institutionID == null)
                        return -1;

                    //Console.WriteLine($"Inserted institution: id:{Convert.ToInt32(institutionID)}, {name}");

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

        [Obsolete("Use the LoadColleagueEducationHistory1 method instead.")]
        /// <summary>
        /// Loads the education history of a colleague into the Account class. This method populates the Account.EducationHistory list.
        /// </summary>
        /// <param name="account">The Account object to which the education history will be associated.</param>
        /// <param name="useCache">A boolean parameter indicating whether the cached data should be used or not. The default value is false.</param>
        /// <returns>Returns true if the education history was loaded successfully, false otherwise.</returns>
        public static bool LoadColleagueEducationHistory(Account account, bool useCache = false)
        {
            if (account == null || account.RecordID < 0)
                return false;

            //temp caches
            Dictionary<int, string> educationTypes = new(), institutionNames = new();

            //create skills list
            StringBuilder sqlEduType = new StringBuilder("SELECT id, type FROM education_types WHERE id IN ("),
                sqlInsti = new StringBuilder("SELECT id, name FROM institutions WHERE id IN (");
            List<EducationData> list;


            string sql;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                sql = @"SELECT id, colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description FROM education_history WHERE (colleague_id=@colleague_id);";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<EducationData>();

                        while (r.Read())
                        {
                            EducationData ed = new EducationData(account, GetInt32(r, 0));
                            ed.EducationTypeID = GetInt32(r, 2);
                            ed.InstitutionID = GetInt32(r, 3);
                            ed.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));

                            ed.StartDate = GetDateOnly(r, 6);
                            ed.EndDate = GetDateOnly(r, 7);

                            ed.Description = GetString(r, 8);

                            list.Add(ed);

                            //add education_type_id
                            if (educationTypes.TryAdd(ed.EducationTypeID, null))
                            {
                                //new id, add it
                                sqlEduType.Append(ed.EducationTypeID);
                                sqlEduType.Append(',');
                            }

                            //add institution_id
                            if (institutionNames.TryAdd(ed.InstitutionID, null))
                            {
                                //new id, add it
                                sqlInsti.Append(ed.InstitutionID);
                                sqlInsti.Append(',');
                            }
                        }

                        //remove last comma, then append ending
                        sqlEduType.Remove(sqlEduType.Length - 1, 1).Append(");");
                        sqlInsti.Remove(sqlInsti.Length - 1, 1).Append(");");

                        account.EducationHistory = list;
                    }
                }

                //no records found.
                if (account.EducationHistory == null || account.EducationHistory.Count < 1)
                    return false;

                //Console.WriteLine($"Found Education Records: {list.Count}");

                //load edu types.
                using (SqliteCommand cmd = new SqliteCommand(sqlEduType.ToString(), conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //fill edutype dict
                        while (r.Read())
                            educationTypes[GetInt32(r, 0)] = GetString(r, 1);

                    }
                }
                //Console.WriteLine($"Found EducationTypes: {educationTypes.Count}");

                //load institution names.
                using (SqliteCommand cmd = new SqliteCommand(sqlInsti.ToString(), conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //fill institution dict
                        while (r.Read())
                            institutionNames[GetInt32(r, 0)] = GetString(r, 1);

                    }
                }

                //Console.WriteLine($"Found Institutions: {institutionNames.Count}");
            }

            //these should always have at least 1 count if one record is loaded.
            if (educationTypes.Count == 0 || institutionNames.Count == 0)
                return false;

            //fill data
            foreach (EducationData ed in list)
            {
                ed.EducationType = educationTypes[ed.EducationTypeID];
                ed.Institution = institutionNames[ed.InstitutionID];
            }

            //Console.WriteLine($"Loaded {list.Count} education records!");

            return true;
        }

        public static bool LoadColleagueEducationHistory1(Account account, bool useCache = false)
        {
            if (account == null || account.RecordID < 0)
                return false;


            List<EducationData> list;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();


                string sql = @"SELECT eh.id, eh.colleague_id, eh.education_type_id, eh.institution_id, eh.municipality_id, eh.state_id, eh.start_date, eh.end_date, eh.description, et.type AS education_type, i.name AS institution
                                            FROM education_history eh
                                            JOIN education_types et ON eh.education_type_id = et.id
                                            JOIN institutions i ON eh.institution_id = i.id
                                            WHERE eh.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<EducationData>();

                        while (r.Read())
                        {
                            EducationData ed = new EducationData(account, GetInt32(r, 0));
                            ed.EducationTypeID = GetInt32(r, 2);
                            ed.InstitutionID = GetInt32(r, 3);
                            ed.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                            ed.StartDate = GetDateOnly(r, 6);
                            ed.EndDate = GetDateOnly(r, 7);
                            ed.Description = GetString(r, 8);
                            ed.EducationType = GetString(r, 9);
                            ed.Institution = GetString(r, 10);

                            list.Add(ed);
                        }

                        account.EducationHistory = list;
                    }
                }
            }

            //Console.WriteLine($"Found Education Records: {list?.Count}");

            return true;
        }



        public static bool LoadColleagueEducationHistory1(Account account, out Dictionary<int, EducationData> dict, bool useCache = false)
        {
            dict = new Dictionary<int, EducationData>();

            if (account == null || account.RecordID < 0)
                return false;


            List<EducationData> list;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();


                string sql = @"SELECT eh.id, eh.colleague_id, eh.education_type_id, eh.institution_id, eh.municipality_id, eh.state_id, eh.start_date, eh.end_date, eh.description, et.type AS education_type, i.name AS institution
                                            FROM education_history eh
                                            JOIN education_types et ON eh.education_type_id = et.id
                                            JOIN institutions i ON eh.institution_id = i.id
                                            WHERE eh.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<EducationData>();

                        EducationData ed;
                        while (r.Read())
                        {
                            ed = new EducationData(account, GetInt32(r, 0));
                            ed.EducationTypeID = GetInt32(r, 2);
                            ed.InstitutionID = GetInt32(r, 3);
                            ed.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                            ed.StartDate = GetDateOnly(r, 6);
                            ed.EndDate = GetDateOnly(r, 7);
                            ed.Description = GetString(r, 8);
                            ed.EducationType = GetString(r, 9);
                            ed.Institution = GetString(r, 10);

                            list.Add(ed);
                            dict.TryAdd(ed.RecordID, ed);
                        }

                        account.EducationHistory = list;
                        return true;
                    }
                }
            }

            //Console.WriteLine($"Found Education Records: {list?.Count}");

            return true;
        }
        #endregion


        #region Work Data

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

                    //Console.WriteLine($"Inserting work_history for {colleagueID}...");
                    //TODO fix empty values
                    object? workID = cmd.ExecuteScalar();

                    if (workID == null)
                        return -1;

                    //Console.WriteLine($"Inserted work_history for {colleagueID}, recordID: {Convert.ToInt32(workID)}");
                    return Convert.ToInt32(workID);//return record ID
                }
            }
        }


        public static int InsertOrIgnoreWorkHistory(int colleagueID, string employer, string jobTitle, string municipality, int stateID, DateOnly startDate, DateOnly endDate, string description)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int workHistoryID = -1, employerID, jobTitleID, municipalityID;

                        string sql = @"INSERT INTO OR IGNORE work_history (colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description) 
                                        VALUES (@colleague_id, @employer_id, @job_title_id, @municipality_id, @state_id, @start_date, @end_date, @description);
                                        SELECT last_insert_rowid();";

                        using (SqliteCommand cmd = new SqliteCommand("", conn, transaction))
                        {
                            employerID = InsertOrIgnore(cmd, "employers", "name", employer);
                            jobTitleID = InsertOrIgnore(cmd, "job_titles", "title", jobTitle);
                            municipalityID = InsertOrIgnore(cmd, "municipalities", "name", municipality);


                            cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(colleagueID));
                            cmd.Parameters.AddWithValue("@employer_id", ValueCleaner(employerID));
                            cmd.Parameters.AddWithValue("@job_title_id", ValueCleaner(jobTitleID));
                            cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(municipalityID));
                            cmd.Parameters.AddWithValue("@state_id", ValueCleaner(stateID));
                            cmd.Parameters.AddWithValue("@start_date", ValueCleaner(startDate));
                            cmd.Parameters.AddWithValue("@end_date", ValueCleaner(endDate));
                            cmd.Parameters.AddWithValue("@description", ValueCleaner(description));

                            object? id = cmd.ExecuteScalar();

                            if (id == null)
                                return -1;

                            workHistoryID = Convert.ToInt32(id);

                        }

                        transaction.Commit();

                        return workHistoryID;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        //Console.WriteLine(ex.Message);
                        throw;
                    }
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


        public static bool SaveWorkHistory(WorkData wd)
        {

            if (wd.Remove)
            {
                return wd.IsRemoved = DeleteRecord("work_history", wd.RecordID);
            }


            wd.EmployerID = SaveEmployer(wd.Employer);
            wd.JobTitleID = SaveJobTitle(wd.JobTitle);

            if (ExistsWorkHistory(wd.RecordID))
                return UpdateWorkHistory(wd) >= 0;

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

        /// <summary>
        /// Retrieves the employer names based on the ids.
        /// </summary>
        /// <param name="ids">the employer ids</param>
        /// <returns>an array with employer ids</returns>
        public static string[] GetEmployers(params int[] ids)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = $"SELECT name FROM employers WHERE (id={string.Join(",", ids)});";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        List<string> employerNames = new List<string>();
                        while (r.Read())
                            employerNames.Add(GetString(r, 0));

                        return employerNames.ToArray();
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
                    //Console.WriteLine($"Inserting employer: {name}");

                    object? employerID = cmd.ExecuteScalar();

                    if (employerID == null)
                        return -1;

                    //Console.WriteLine($"Inserted employer: id:{Convert.ToInt32(employerID)}, {name}");

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

                    //Console.WriteLine($"Inserting job_title: {title}");

                    object? jobTitleID = cmd.ExecuteScalar();

                    if (jobTitleID == null)
                        return -1;
                    //Console.WriteLine($"Inserted job_title: id:{Convert.ToInt32(jobTitleID)}, {title}");

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



        [Obsolete("Use LoadColleagueWorkHistory1 instead.")]
        public static bool LoadColleagueWorkHistory(Account account, bool useCache = false)
        {
            if (account == null || account.RecordID < 0)
                return false;

            //temp caches
            Dictionary<int, string> employers = new(), jobTitles = new();

            //create work history list
            StringBuilder sqlEmp = new("SELECT id, name FROM employers WHERE id IN ("),
                sqlJob = new("SELECT id, title FROM job_titles WHERE id IN (");

            List<WorkData> list;

            string sql;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();
                sql = @"SELECT id, colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description FROM work_history WHERE (colleague_id=@colleague_id);";

                using (SqliteCommand cmd = new(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<WorkData>();

                        while (r.Read())
                        {
                            WorkData wd = new WorkData(account, GetInt32(r, 0));
                            wd.EmployerID = GetInt32(r, 2);
                            wd.JobTitleID = GetInt32(r, 3);
                            wd.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));

                            wd.StartDate = GetDateOnly(r, 6);
                            wd.EndDate = GetDateOnly(r, 7);

                            wd.Description = GetString(r, 8);

                            list.Add(wd);

                            //add employer_id
                            if (employers.TryAdd(wd.EmployerID, null))
                            {
                                //new id, add it
                                sqlEmp.Append(wd.EmployerID);
                                sqlEmp.Append(',');
                            }

                            //add job_title_id
                            if (jobTitles.TryAdd(wd.JobTitleID, null))
                            {
                                //new id, add it
                                sqlJob.Append(wd.JobTitleID);
                                sqlJob.Append(',');
                            }

                        }

                        //emove last comma, then append ending
                        sqlEmp.Remove(sqlEmp.Length - 1, 1).Append(");");
                        sqlJob.Remove(sqlJob.Length - 1, 1).Append(");");

                        account.WorkHistory = list;
                    }
                }

                //no records found
                if (account.WorkHistory == null || account.WorkHistory.Count < 1)
                    return false;

                //Console.WriteLine($"Found Work Records: {list.Count}");

                //load employers
                using (SqliteCommand cmd = new SqliteCommand(sqlEmp.ToString(), conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //fill employer dict
                        while (r.Read())
                            employers[GetInt32(r, 0)] = GetString(r, 1);

                    }
                }

                //Console.WriteLine($"Found Employers: {employers.Count}");

                //load job titles.
                using (SqliteCommand cmd = new SqliteCommand(sqlJob.ToString(), conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //fill job title dict
                        while (r.Read())
                            jobTitles[GetInt32(r, 0)] = GetString(r, 1);

                    }
                }

                //Console.WriteLine($"Found Job Titles: {jobTitles.Count}");
            }

            //these should always have at least 1 count if one record is loaded.
            if (employers.Count == 0 || jobTitles.Count == 0)
                return false;

            //fill data
            foreach (WorkData wd in list)
            {
                wd.Employer = employers[wd.EmployerID];
                wd.JobTitle = jobTitles[wd.JobTitleID];
            }

            //Console.WriteLine($"Loaded {list.Count} work experience records!");

            return true;

        }



        public static bool LoadColleagueWorkHistory1(Account account, bool useCache = false)
        {
            if (account == null || account.RecordID < 0)
                return false;

            //create work history list
            List<WorkData> list;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT wh.id, wh.colleague_id, wh.employer_id, wh.job_title_id, wh.municipality_id, wh.state_id, wh.start_date, wh.end_date, wh.description, e.name AS employer_name, jt.title AS job_title_name
                                FROM work_history wh
                                JOIN employers e ON wh.employer_id = e.id
                                JOIN job_titles jt ON wh.job_title_id = jt.id
                                WHERE wh.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<WorkData>();

                        while (r.Read())
                        {
                            WorkData wd = new WorkData(account, GetInt32(r, 0));
                            wd.EmployerID = GetInt32(r, 2);
                            wd.JobTitleID = GetInt32(r, 3);
                            wd.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                            wd.StartDate = GetDateOnly(r, 6);
                            wd.EndDate = GetDateOnly(r, 7);
                            wd.Description = GetString(r, 8);
                            wd.Employer = GetString(r, 9);
                            wd.JobTitle = GetString(r, 10);
                            list.Add(wd);
                        }

                        account.WorkHistory = list;
                    }
                }
            }


            //Console.WriteLine($"Found Work Records: {list?.Count}");

            return true;
        }

        public static bool LoadColleagueWorkHistory1(Account account, out Dictionary<int, WorkData> dict, bool useCache = false)
        {
            dict = new();

            if (account == null || account.RecordID < 0)
                return false;

            //create work history list
            List<WorkData> list;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT wh.id, wh.colleague_id, wh.employer_id, wh.job_title_id, wh.municipality_id, wh.state_id, wh.start_date, wh.end_date, wh.description, e.name AS employer_name, jt.title AS job_title_name
                                FROM work_history wh
                                JOIN employers e ON wh.employer_id = e.id
                                JOIN job_titles jt ON wh.job_title_id = jt.id
                                WHERE wh.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<WorkData>();
                        WorkData wd;
                        while (r.Read())
                        {
                            wd = new WorkData(account, GetInt32(r, 0));
                            wd.EmployerID = GetInt32(r, 2);
                            wd.JobTitleID = GetInt32(r, 3);
                            wd.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                            wd.StartDate = GetDateOnly(r, 6);
                            wd.EndDate = GetDateOnly(r, 7);
                            wd.Description = GetString(r, 8);
                            wd.Employer = GetString(r, 9);
                            wd.JobTitle = GetString(r, 10);

                            list.Add(wd);
                            dict.TryAdd(wd.RecordID, wd);
                        }

                        account.WorkHistory = list;
                    }
                }
            }


            //Console.WriteLine($"Found Work Records: {list?.Count}");

            return true;
        }
        #endregion




        #region Profile Methods

        public static int InsertProfile(ProfileData pd)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int profileID = -1;

                        string sql = @"INSERT INTO profiles (colleague_id, title, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering) 
                                    VALUES (@colleague_id, @title, @colleague_skills_ids, @education_history_ids, @colleague_certs_ids, @work_history_ids, @ordering)
                                    RETURNING id;";

                        using (SqliteCommand cmd = new SqliteCommand(sql, conn, transaction))
                        {
                            /*
                            Console.WriteLine("[Profile=" + pd.Title + "] {");
                            Console.WriteLine("Skills: " + ValueCleaner(string.Join(",", pd.SelectedSkillIDs)));
                            Console.WriteLine("Education: " + ValueCleaner(string.Join(",", pd.SelectedEducationIDs)));
                            Console.WriteLine("Certs: " + ValueCleaner(string.Join(",", pd.SelectedCertificationIDs)));
                            Console.WriteLine("Work: " + ValueCleaner(string.Join(",", pd.SelectedWorkIDs)));
                            Console.WriteLine("}");//*/

                            cmd.Parameters.AddWithValue("@id", ValueCleaner(pd.RecordID));
                            cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(pd.Owner.RecordID));
                            cmd.Parameters.AddWithValue("@title", ValueCleaner(pd.Title));
                            cmd.Parameters.AddWithValue("@colleague_skills_ids", ValueCleaner(string.Join(",", pd.SelectedSkillIDs)));
                            cmd.Parameters.AddWithValue("@education_history_ids", ValueCleaner(string.Join(",", pd.SelectedEducationIDs)));
                            cmd.Parameters.AddWithValue("@colleague_certs_ids", ValueCleaner(string.Join(",", pd.SelectedCertificationIDs)));
                            cmd.Parameters.AddWithValue("@work_history_ids", ValueCleaner(string.Join(",", pd.SelectedWorkIDs)));
                            cmd.Parameters.AddWithValue("@ordering", ValueCleaner(pd.Ordering));

                            object? id = cmd.ExecuteScalar();

                            if (id == null)
                                return -1;

                            pd.RecordID = profileID = Convert.ToInt32(id);

                        }

                        transaction.Commit();

                        return profileID;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
        }


        public static bool UpdateProfile(ProfileData pd)
        {
            bool success = false;
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int profileID = -1;

                        string sql = @"UPDATE profiles SET colleague_id=@colleague_id, title=@title, colleague_skills_ids=@colleague_skills_ids, education_history_ids=@education_history_ids, colleague_certs_ids=@colleague_certs_ids, work_history_ids=@work_history_ids, ordering=@ordering 
                                    WHERE id=@id;";

                        using (SqliteCommand cmd = new SqliteCommand(sql, conn, transaction))
                        {
                            /*
                            Console.WriteLine("[Profile=" + pd.Title + "] {");
                            Console.WriteLine("Skills: " + ValueCleaner(string.Join(",", pd.SelectedSkillIDs)));
                            Console.WriteLine("Education: " + ValueCleaner(string.Join(",", pd.SelectedEducationIDs)));
                            Console.WriteLine("Certs: " + ValueCleaner(string.Join(",", pd.SelectedCertificationIDs)));
                            Console.WriteLine("Work: " + ValueCleaner(string.Join(",", pd.SelectedWorkIDs)));
                            Console.WriteLine("}");//*/

                            cmd.Parameters.AddWithValue("@id", ValueCleaner(pd.RecordID));
                            cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(pd.Owner.RecordID));
                            cmd.Parameters.AddWithValue("@title", ValueCleaner(pd.Title));
                            cmd.Parameters.AddWithValue("@colleague_skills_ids", ValueCleaner(string.Join(",", pd.SelectedSkillIDs)));
                            cmd.Parameters.AddWithValue("@education_history_ids", ValueCleaner(string.Join(",", pd.SelectedEducationIDs)));
                            cmd.Parameters.AddWithValue("@colleague_certs_ids", ValueCleaner(string.Join(",", pd.SelectedCertificationIDs)));
                            cmd.Parameters.AddWithValue("@work_history_ids", ValueCleaner(string.Join(",", pd.SelectedWorkIDs)));
                            cmd.Parameters.AddWithValue("@ordering", ValueCleaner(pd.Ordering));

                            success = cmd.ExecuteNonQuery() >= 0;

                        }

                        transaction.Commit();

                        return success;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
        }

        public static bool LoadColleageProfiles(Account account)
        {
            if (account == null || account.RecordID < 0)
                return false;

            //create work history list
            List<ProfileData> list;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT id, colleague_id, title, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering
                                FROM profiles
                                WHERE colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<ProfileData>();

                        ProfileData pd;
                        string skillIDs, educationIDs, certIDs, workIDs;
                        HashSet<int> skills, education, certs, work;

                        void CreateHashSet(string values, out HashSet<int> hashSet)
                        {
                            if (string.IsNullOrEmpty(values))
                                hashSet = new HashSet<int>();
                            else if (values.Length == 1)
                                (hashSet = new HashSet<int>()).Add(int.Parse(values));
                            else
                                hashSet = new HashSet<int>(values.Split(',').Select(int.Parse));
                        }


                        while (r.Read())
                        {
                            skillIDs = GetString(r, 3);
                            educationIDs = GetString(r, 4);
                            certIDs = GetString(r, 5);
                            workIDs = GetString(r, 6);

                            CreateHashSet(skillIDs, out skills);
                            CreateHashSet(educationIDs, out education);
                            CreateHashSet(certIDs, out certs);
                            CreateHashSet(workIDs, out work);

                            list.Add(pd = new ProfileData(
                                account,
                                GetInt32(r, 0), //populate profile record id
                                GetString(r, 2), //populate title
                                skills, //populate skills ids
                                education, //populate education history ids
                                certs, //populate certification ids
                                work, //populate work history ids
                                GetString(r, 6) //populate ordering
                                ));

                        }

                        account.Profiles = list;
                    }
                }
            }


            //Console.WriteLine($"Found Profile Records: {list?.Count}");

            return true;
        }

        public static bool LoadColleageProfiles(Account account, out Dictionary<int, ProfileData> dict)
        {
            dict = new Dictionary<int, ProfileData>();

            if (account == null || account.RecordID < 0)
                return false;

            //create work history list
            List<ProfileData> list;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT id, colleague_id, title, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering
                                FROM profiles
                                WHERE colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        list = new List<ProfileData>();

                        ProfileData pd;
                        string skillIDs, educationIDs, certIDs, workIDs;
                        HashSet<int> skills, education, certs, work;

                        void CreateHashSet(string values, out HashSet<int> hashSet)
                        {
                            if (string.IsNullOrEmpty(values))
                                hashSet = new HashSet<int>();
                            else if (values.Length == 1)
                                (hashSet = new HashSet<int>()).Add(int.Parse(values));
                            else
                                hashSet = new HashSet<int>(values.Split(',').Select(int.Parse));
                        }

                        while (r.Read())
                        {
                            skillIDs = GetString(r, 3);
                            educationIDs = GetString(r, 4);
                            certIDs = GetString(r, 5);
                            workIDs = GetString(r, 6);

                            CreateHashSet(skillIDs, out skills);
                            CreateHashSet(educationIDs, out education);
                            CreateHashSet(certIDs, out certs);
                            CreateHashSet(workIDs, out work);


                            list.Add(pd = new ProfileData(
                                account,
                                GetInt32(r, 0), //populate profile record id
                                GetString(r, 2), //populate title
                                skills, //populate skills ids
                                education, //populate education history ids
                                certs, //populate certification ids
                                work, //populate work history ids
                                GetString(r, 6) //populate ordering
                                ));

                            dict.TryAdd(pd.RecordID, pd);
                        }

                        account.Profiles = list;
                    }
                }
            }


            //Console.WriteLine($"Found Profile Records: {dict?.Count}");

            return true;
        }


        public static bool SaveProfile(ProfileData pd)
        {
            if (pd.Remove)
            {
                /*if (pd.RecordID < 1)
                {
                    pd.IsRemoved = true;
                    Console.WriteLine("#####Removed unsaved profile: " + pd.RecordID + " " + pd.IsRemoved);
                    return pd.IsRemoved;
                }//*/

                pd.IsRemoved = DeleteRecord("profiles", pd.RecordID);
                return pd.IsRemoved;
            }

            if (pd.RecordID > 0)
                return UpdateProfile(pd);

            return (pd.RecordID = InsertProfile(pd)) >= 0;
        }
        #endregion


        #region Certification methods

        public static int SaveCertificationType(string certificationType)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand("", conn))
                {
                    return InsertOrIgnore(cmd, "cert_types", "type", certificationType);
                }
            }
        }


        public static int InsertCertification(CertificationData cd)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO 
                                colleague_certs(colleague_id, cert_type_id, institution_id, municipality_id, state_id, start_date, end_date, description) 
                                VALUES (@colleague_id, @cert_type_id, @institution_id, @municipality_id, @state_id, @start_date, @end_date, @description)
                                RETURNING id;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", cd.Owner.RecordID);
                    cmd.Parameters.AddWithValue("@cert_type_id", cd.CertificationTypeID);
                    cmd.Parameters.AddWithValue("@institution_id", cd.InstitutionID);
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(cd.Location.MunicipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(cd.Location.StateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(cd.StartDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(cd.EndDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(cd.Description));

                    //Console.WriteLine($"Inserting colleague_cert for {cd.Owner.RecordID} certtypeid: {cd.CertificateTypeID}, instid: {cd.InstitutionID}...");
                    object? id = cmd.ExecuteScalar();

                    if (id == null)
                        return -1;

                    //Console.WriteLine($"Inserted colleague_cert for {cd.Owner.RecordID}, recordID: {Convert.ToInt32(id)}");

                    return Convert.ToInt32(id);//return record ID
                }
            }
        }

        public static bool UpdateCertification(CertificationData cd)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE colleague_certs SET 
                                colleague_id=@colleague_id, cert_type_id=@cert_type_id, institution_id=@institution_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description WHERE id = @id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", cd.RecordID);
                    cmd.Parameters.AddWithValue("@colleague_id", cd.Owner.RecordID);
                    cmd.Parameters.AddWithValue("@cert_type_id", cd.CertificationTypeID);
                    cmd.Parameters.AddWithValue("@institution_id", cd.InstitutionID);
                    cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(cd.Location.MunicipalityID));
                    cmd.Parameters.AddWithValue("@state_id", ValueCleaner(cd.Location.StateID));
                    cmd.Parameters.AddWithValue("@start_date", ValueCleaner(cd.StartDate));
                    cmd.Parameters.AddWithValue("@end_date", ValueCleaner(cd.EndDate));
                    cmd.Parameters.AddWithValue("@description", ValueCleaner(cd.Description));
                    return cmd.ExecuteNonQuery() >= 0;//rows affected
                }
            }
        }

        public static bool SaveCertification(CertificationData cd)
        {

            if (cd.Remove)
            {
                //delete cert
                return cd.IsRemoved = DeleteRecord("colleague_certs", cd.RecordID);
            }

            cd.CertificationTypeID = SaveCertificationType(cd.CertificationType);
            cd.InstitutionID = SaveInstitution(cd.Institution);

            //Thread.Sleep(1);
            if (cd.RecordID > 0)
                return UpdateCertification(cd);
            return (cd.RecordID = InsertCertification(cd)) >= 0;
            //return SaveEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationType, ed.Institution, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }


        public static bool LoadColleagueCertifications(Account account, out Dictionary<int, CertificationData> dict)
        {
            dict = new Dictionary<int, CertificationData>();

            if (account == null || account.RecordID < 0)
                return false;



            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();


                string sql = @"SELECT cc.id, cc.colleague_id, cc.cert_type_id, cc.institution_id, cc.municipality_id, cc.state_id, cc.start_date, cc.end_date, cc.description, ct.type AS cert_type, i.name AS institution
                                            FROM colleague_certs cc
                                            JOIN cert_types ct ON cc.cert_type_id = ct.id
                                            JOIN institutions i ON cc.institution_id = i.id
                                            WHERE cc.colleague_id=@colleague_id;";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colleague_id", account.RecordID);

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        CertificationData cd;
                        while (r.Read())
                        {
                            //Account owner, int recordID, string institution, int institutionID, string certificateType, int certificateTypeID, string? description, Location? location, DateOnly? startDate, DateOnly? endDate
                            cd = new CertificationData(
                                account,
                                GetInt32(r, 0),
                                GetString(r, 10),
                                GetInt32(r, 3),
                                GetString(r, 9),
                                GetInt32(r, 2),
                                GetString(r, 8),
                                new Location(GetInt32(r, 4), GetInt32(r, 5)),
                                GetDateOnly(r, 6),
                                GetDateOnly(r, 7)
                                );

                            dict.TryAdd(cd.RecordID, cd);
                        }

                    }
                }
            }

            //Console.WriteLine($"Found Certification Records: {dict?.Count}");

            return true;
        }

        #endregion







        #region Debugging

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
                                //r.GetOrdinal(r.GetName(i));
                                s += $"{r.GetOrdinal(r.GetName(i))}:{r[i]}\\";
                            }
                            list.Add(s);
                        }


                        return list;
                    }
                }
            }
        }

        public static void Thomas(Account account)
        {
            //ww8FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k
            account.UpdateData(
                "Wall, Thomas Joseph",
                "thwall@augusta.edu",
                1231231234,
                @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. "
            );

            account.AddEducation(
                "Harvard",
                "Masters",
                "Business Administration",
                null,
                Utilities.ToDateOnly("August 18 1993"),
                null
                );

            account.AddEducation(
                "Aiken Technical College",
                "Bachelor of Arts",
                "Computer Design",
                null,
                Utilities.ToDateOnly("August 15 1990"),
                null
                );

            account.AddEducation(
                "North Augusta High School",
                "Scrum Master",
                "in my Mom's Kitchen",
                null,
                Utilities.ToDateOnly("August 8 1980"),
                null
                );

            account.AddWork(
                "The Kern Family Foundation",
                "Chair Sitting Assistant",
                "sat in chairs",
                null,
                Utilities.ToDateOnly("September 1 2006"),
                Utilities.ToDateOnly("September 1 2009")
                );
            account.AddWork(
                "Management Research Service",
                "Social Media Influencer",
                "made money for absolutely no reason",
                null,
                Utilities.ToDateOnly("October 12 2009"),
                Utilities.ToDateOnly("April 1 2023")
                );

            account.AddSkills("Programming Lanaguages", "Java", "C#", "HTML");

            account.AddCertification("Online", "CompTIA S+", null, null);

            string s;
            for (int i = 0; i < 10; i ++)
            {
                s = $"{i}{i}{i}";
                account.AddEducation(
                    $"TestInstitution{s}",
                    $"TestDegreeType{s}",
                    $"TestDegreeField{s}",
                    null,
                    Utilities.ToDateOnly($"May {i + 1} 200{i}"),
                    Utilities.ToDateOnly($"August {i + 1} 200{i}")
                    );
                account.AddWork(
                    $"TestEmployer{s}",
                    $"TestJobTitle{s}",
                    $"TestJobDescription{s}",
                    null,
                    Utilities.ToDateOnly($"Jan {i+1} 200{i}"),
                    Utilities.ToDateOnly($"April {i + 1} 200{i}")
                    );
                account.AddCertification(
                    $"TestInstitution{s}",
                    $"TestCertType{s}",
                    Utilities.ToDateOnly($"Oct {i + 1} 200{i}"),
                    Utilities.ToDateOnly($"Sept {i + 1} 200{i}")
                    );
                account.AddSkills($"TestCategory{s}", $"TestSkill{s}");
            }

            //must persit all before making profiles
            account.PersistAll();




            //create mock profile
            ProfileData p = account.CreateProfile("Mister Sir"), lastP;
            p.AddWork(1);
            p.AddWork(2);
            p.AddEducation(2);
            p.AddEducation(1);
            p.AddSkill(3);
            lastP = p;
            account.CreateProfile("Main");

            Random r = new Random();
            for (int i = 0; i < 10; i ++)
            {
                s = $"{i}{i}{i}";
                p = account.CreateProfile($"TestProfile{s}");
                p.AddWork(r.Next(1, 10));
                p.AddWork(r.Next(1, 10));
                p.AddEducation(r.Next(1, 10));
                p.AddEducation(r.Next(1, 10));
                p.AddSkill(r.Next(1, 10));
            }
            account.PersistAll();



            Console.WriteLine("Test: Attempting to remove profileID: " + lastP.RecordID);
            account.RemoveProfiles(lastP.RecordID);
            account.PersistAll();


            Console.WriteLine("Test: Attempting to some test data.");
            account.RemoveSkills(5, 6, 7);
            account.RemoveCertifications(5, 6, 7);
            account.RemoveEducations(5, 6, 7);
            account.RemoveWorks(5, 6, 7);
            account.PersistAll();

            Console.WriteLine("Saved!");
        }
        #endregion


        #region Sqlite Helper Methods

        /// <summary>
        /// Uses an INSERT OR IGNORE query, then returns the result that was input. Only intended for tables with an id and unique text column.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns>The id of the value inserted into the input table</returns>
        protected static int InsertOrIgnore(SqliteCommand cmd, string tableName, string fieldName, string value)
        {
            //save incoming command type
            CommandType originalType = cmd.CommandType;
            string originalSql = cmd.CommandText;

            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $"INSERT OR IGNORE INTO {tableName} ({fieldName}) VALUES (@val); SELECT id FROM {tableName} WHERE {fieldName} = @val;";

            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));


            object? o = cmd.ExecuteScalar();

            //reset command type
            cmd.CommandType = originalType;
            cmd.CommandText = originalSql;

            if (o == null)
                return -1;


            return Convert.ToInt32(o);
        }


        protected static List<int> InsertOrIgnore(SqliteCommand cmd, string tableName, string fieldName, List<string> values)
        {
            //save incoming command type
            CommandType originalType = cmd.CommandType;
            string originalSql = cmd.CommandText;

            cmd.CommandType = CommandType.Text;

            //build the SQL query string for inserting the values
            StringBuilder sb = new StringBuilder($"INSERT OR IGNORE INTO {tableName} ({fieldName}) VALUES ");

            //insert value parameters into query
            for (int i = 0; i < values.Count; i++)
                sb.Append($"@val{i},");
            sb.Remove(sb.Length - 1, 1);

            sb.Append($"; SELECT id FROM {tableName} WHERE {fieldName} IN ({string.Join(",", values)});");

            //insert values into query
            cmd.CommandText = sb.ToString();
            for (int i = 0; i < values.Count; i++)
                cmd.Parameters.AddWithValue($"@val{i}", ValueCleaner(values[i]));


            List<int> ids = new List<int>();
            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                    ids.Add(GetInt32(r, 0));
            }

            //reset command type
            cmd.CommandType = originalType;
            cmd.CommandText = originalSql;

            return ids;
        }

        //Does not work correctly for some reason? May be 
        private static bool DeleteRecord(SqliteCommand cmd, string tableName, int id)
        {

            //save incoming command type
            CommandType originalType = cmd.CommandType;
            string originalSql = cmd.CommandText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"DELETE FROM {tableName} WHERE id={id};";

            //how many rows were deleted? 0 is find since the where condition may have been invalid.
            bool success = cmd.ExecuteNonQuery() >= 0;

            //reset command type
            cmd.CommandType = originalType;
            cmd.CommandText = originalSql;


            return success;
        }

        private static bool DeleteRecords(SqliteCommand cmd, string tableName, params int[] ids)
        {

            //save incoming command type
            CommandType originalType = cmd.CommandType;
            string originalSql = cmd.CommandText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"DELETE FROM {tableName} WHERE id IN ({string.Join(',', ids)});";


            bool success = cmd.ExecuteNonQuery() >= 0;

            //reset command type
            cmd.CommandType = originalType;
            cmd.CommandText = originalSql;


            return success;
        }

        private static bool DeleteRecord(string tableName, int id)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                string sql = $"DELETE FROM {tableName} WHERE id={id};";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery() >= 0;
                }
            }
        }

        private static bool DeleteRecords(string tableName, params int[] ids)
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();

                string sql = $"DELETE FROM {tableName} WHERE id IN ({string.Join(',', ids)});";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {

                    return cmd.ExecuteNonQuery() >= 0;
                }
            }
        }


    private static object ValueCleaner(int val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }

        private static object ValueCleaner(long val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }

        private static object ValueCleaner(string val)
        {
            if (val == null)
                return DBNull.Value;

            //Test before using.

            return val;//Utilities.HtmlStripper(val);
        }

        private static object ValueCleaner(DateOnly? val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }

        private static long GetInt64(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt64(ordinal);
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
        #endregion




        ///<summary>
        ///This is initial query used for creating the initial database and tables.
        ///Should only be used if the database is being completely reset or initially created.
        ///</summary>
        private const string dbSQL = @"
        PRAGMA foreign_keys = 0;
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
        DROP TABLE IF EXISTS [skill_categories];
        DROP TABLE IF EXISTS [colleague_skills];
        DROP TABLE IF EXISTS [cert_types];
        DROP TABLE IF EXISTS [colleague_certs];
        DROP TABLE IF EXISTS [profiles];
        COMMIT;
        PRAGMA foreign_keys = 1;

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
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE states (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL,
          abbreviation CHAR(2)
        );

        CREATE TABLE institutions (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE education_types (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          type TEXT UNIQUE NOT NULL
        );

        CREATE TABLE education_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          education_type_id INTEGER NOT NULL,
          institution_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (education_type_id) REFERENCES education_types(id),
          FOREIGN KEY (institution_id) REFERENCES institutions(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id)
        );
        
        CREATE TABLE cert_types (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          type TEXT UNIQUE NOT NULL
        );
        
        CREATE TABLE colleague_certs (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          cert_type_id INTEGER NOT NULL,
          institution_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (cert_type_id) REFERENCES cert_types(id),
          FOREIGN KEY (institution_id) REFERENCES institutions(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id)
        );


        CREATE TABLE employers (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE job_titles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          title TEXT UNIQUE NOT NULL
        );

        CREATE TABLE work_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          employer_id INTEGER NOT NULL,
          job_title_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (employer_id) REFERENCES employers(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id),
          FOREIGN KEY (job_title_id) REFERENCES job_titles(id)
          );

        CREATE TABLE skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE skill_categories (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE colleague_skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          skill_id INTEGER NOT NULL,
          skill_category_id INTEGER,
          rating INTEGER,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (skill_id) REFERENCES skills(id),
          FOREIGN KEY (skill_category_id) REFERENCES skill_categories(id)
        );

        CREATE TABLE profiles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          title TEXT NOT NULL,          --similar to the file name
          colleague_skills_ids TEXT,    --list of colleague_skills ids in specified order
          education_history_ids TEXT,   --list of education history ids in specified order
          colleague_certs_ids TEXT,     --list of colleague_certs ids in specified order
          work_history_ids TEXT,        --list of work history ids in specified order
          ordering TEXT,                --The ordering of how the different sections will be (education, work, skills, etc)
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE
        );

        ALTER TABLE colleagues ADD COLUMN main_profile_id INTEGER REFERENCES profiles(id) ON DELETE CASCADE;
        
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

    }

}
