using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;
using System.Text;
using SCCPP1.Database.Tables;
using SCCPP1.Database;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Sqlite;
using System.Reflection;
using SCCPP1.Database.Requests;

namespace SCCPP1
{

    /// <summary>
    /// This class is used to connect to the database and execute queries.
    /// </summary>
    /// <remarks>
    /// This class was to be replaced entirely by the <see cref="DbRequestManager"/> which uses a connection
    /// pool instead of static methods with new object creation. Due to time constraints, the other system was not fully completed
    /// and tested, so, this class is still used as the primary connection to the database.
    /// </remarks>
    public class DatabaseConnector
    {

        private static string connStr = DbConstants.ConnectionString;

        //load these tables in memory to reduce CPU usage
        //may not need to do this
        private static Dictionary<int, string> skills, education_types, institutions, municipalities, employers, job_titles;

        //states are saved as abbreviation on first two chars and the other chars are the full name
        private static Dictionary<int, string> states;


        /// <summary>
        /// Initializes the database by executing the startup SQL script.
        /// </summary>
        public static void InitiateDatabase(bool loadCaches = false)
        {
            string sql = "";

            if (DbConstants.STARTUP_RESET_TABLES)
                sql = _resetTablesSql;

            sql += _schemaCheckSql;

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                Console.WriteLine("Sqlite Server Version: " + conn.ServerVersion);
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqliteException e)
                    {
                        Console.WriteLine($"[{typeof(DatabaseConnector).Name}] handler set.");
                        Console.WriteLine($"[{typeof(DatabaseConnector).Name}] {e.Message}");
                        Console.WriteLine($"[{typeof(DatabaseConnector).Name}] Command: {e.BatchCommand}");
                        Console.WriteLine($"[{typeof(DatabaseConnector).Name}] Data: {e.Data}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[{typeof(DatabaseConnector).Name}] {e.Message}");
                    }


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

        /// <summary>
        /// Loads the skills cache.
        /// </summary>
        private static void LoadCacheSkills()
        {
            skills = LoadTwoColumnTable("skills");
        }


        /// <summary>
        /// Loads the education types cache.
        /// </summary>
        private static void LoadCacheEducationTypes()
        {
            education_types = LoadTwoColumnTable("education_types");
        }


        /// <summary>
        /// Loads the institutions cache.
        /// </summary>
        private static void LoadCacheInstitutions()
        {
            institutions = LoadTwoColumnTable("institutions");
        }


        /// <summary>
        /// Loads the municipalities cache.
        /// </summary>
        private static void LoadCacheMunicipalities()
        {
            municipalities = LoadTwoColumnTable("municipalities");
        }

        
        /// <summary>
        /// Loads the states cache.
        /// </summary>
        private static void LoadCacheStates() //maybe don't need, could just hard code
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


        /// <summary>
        /// Loads the employers cache.
        /// </summary>
        private static void LoadCacheEmployers()
        {
            employers = LoadTwoColumnTable("employers");
        }


        /// <summary>
        /// Loads the job titles cache.
        /// </summary>
        private static void LoadCacheJobTitles()
        {
            job_titles = LoadTwoColumnTable("job_titles");
        }


        /// <summary>
        /// Loads all caches.
        /// </summary>
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


        /// <summary>
        /// Loads a two column table cache.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A dictionary with the table's ID as the key and the value as the string.</returns>
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

        /// <summary>
        /// Gets a cached skill by ID.
        /// </summary>
        /// <param name="id">The ID of the skill to get.</param>
        /// <returns>The cached skill as a string, or null if the skill was not found.</returns>
        private static string? GetCachedSkill(int id)
        {
            return GetCachedValue(skills, id);
        }


        /// <summary>
        /// Gets an array of cached skills by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the skills to get.</param>
        /// <returns>An array of cached skills as strings.</returns>
        private static string[] GetCachedSkills(params int[] ids)
        {
            return GetCachedValues(skills, ids);
        }


        /// <summary>
        /// Gets a cached education type by ID.
        /// </summary>
        /// <param name="id">The ID of the education type to get.</param>
        /// <returns>The cached education type as a string, or null if the education type was not found.</returns>
        private static string? GetCachedEducationType(int id)
        {
            return GetCachedValue(education_types, id);
        }


        /// <summary>
        /// Gets an array of cached education types by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the education types to get.</param>
        /// <returns>An array of cached education types as strings.</returns>
        private static string[] GetCachedEducationTypes(params int[] ids)
        {
            return GetCachedValues(education_types, ids);
        }


        /// <summary>
        /// Gets a cached institution by ID.
        /// </summary>
        /// <param name="id">The ID of the institution to get.</param>
        /// <returns>The cached institution as a string, or null if the institution was not found.</returns>
        private static string? GetCachedInstitution(int id)
        {
            return GetCachedValue(institutions, id);
        }


        /// <summary>
        /// Gets an array of cached institutions by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the institutions to get.</param>
        /// <returns>An array of cached institutions as strings.</returns>
        private static string[] GetCachedInstitutions(params int[] ids)
        {
            return GetCachedValues(institutions, ids);
        }


        /// <summary>
        /// Gets a cached municipality by ID.
        /// </summary>
        /// <param name="id">The ID of the municipality to get.</param>
        /// <returns>The cached municipality as a string, or null if the municipality was not found.</returns>
        public static string? GetCachedMunicipality(int id)
        {
            return GetCachedValue(municipalities, id);
        }


        /// <summary>
        /// Gets an array of cached municipalities by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the municipalities to get.</param>
        /// <returns>An array of cached municipalities as strings.</returns>
        public static string[] GetCachedMunicipalities(params int[] ids)
        {
            return GetCachedValues(municipalities, ids);
        }


        /// <summary>
        /// Gets a cached state by ID.
        /// </summary>
        /// <param name="id">The ID of the state to get.</param>
        /// <returns>The cached state as a string, or null if the state was not found.</returns>
        public static string? GetCachedState(int id)
        {
            return GetCachedValue(states, id);
        }


        /// <summary>
        /// Gets an array of cached states by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the states to get.</param>
        /// <returns>An array of cached states as strings.</returns>
        public static string[] GetCachedStates(params int[] ids)
        {
            return GetCachedValues(states, ids);
        }


        /// <summary>
        /// Gets a cached employer by ID.
        /// </summary>
        /// <param name="id">The ID of the employer to get.</param>
        /// <returns>The cached employer as a string, or null if the employer was not found.</returns>
        private static string? GetCachedEmployer(int id)
        {
            return GetCachedValue(employers, id);
        }


        /// <summary>
        /// Gets an array of cached employers by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the employers to get.</param>
        /// <returns>An array of cached employers as strings.</returns>
        private static string[] GetCachedEmployers(params int[] ids)
        {
            return GetCachedValues(employers, ids);
        }


        /// <summary>
        /// Gets a cached job title by ID.
        /// </summary>
        /// <param name="id">The ID of the job title to get.</param>
        /// <returns>The cached job title as a string, or null if the job title was not found.</returns>
        private static string? GetCachedJobTitle(int id)
        {
            return GetCachedValue(job_titles, id);
        }


        /// <summary>
        /// Gets an array of cached job titles by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the job titles to get.</param>
        /// <returns>An array of cached job titles as strings.</returns>
        private static string[] GetCachedJobTitles(params int[] ids)
        {
            return GetCachedValues(job_titles, ids);
        }


        /// <summary>
        /// Gets a cached value from a given dictionary by ID.
        /// </summary>
        /// <param name="table">The dictionary to get the value from.</param>
        /// <param name="id">The ID of the value to get.</param>
        /// <returns>The cached value as a string, or null if the value was not found.</returns>
        private static string? GetCachedValue(Dictionary<int, string> table, int id)
        {
            string? s;

            if (table.TryGetValue(id, out s))
                return s;

            return null;
        }


        /// <summary>
        /// Gets an array of cached values from a given dictionary by their IDs.
        /// </summary>
        /// <param name="table">The dictionary to get the values from.</param>
        /// <param name="ids">The IDs of the values to get.</param>
        /// <returns>An array of cached values as strings.</returns>
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


        /// <summary>
        /// Attempts to get a cached skill by ID, first searching the dictionary cache, then the database.
        /// </summary>
        /// <param name="id">The ID of the skill to get.</param>
        /// <returns>The cached skill as a string, or null if the skill was not found.</returns>
        private static string? TryGetCachedSkill(int id)
        {
            //check cache, if not there, reload cache
            if (GetCachedSkill(id) == null)
                LoadCacheSkills();

            return GetCachedSkill(id);
        }


        /// <summary>
        /// Gets an array of cached skills by their IDs.
        /// </summary>
        /// <remarks>
        /// This method is not currently used.
        /// </remarks>
        /// <param name="ids">The IDs of the skills to get.</param>
        /// <returns>An array of cached skills as strings.</returns>
        private static string[] TryGetCachedSkills(params int[] ids)
        {
            return GetCachedSkills(ids);
        }


        #endregion

        #region User Data

        /// <summary>
        /// Inserts a new user into the database.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="role"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="introNarrative"></param>
        /// <param name="mainProfileID"></param>
        /// <returns>The ID of the inserted record, -1 if not inserted.</returns>
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


        /// <summary>
        /// Inserts a new account into the database.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>True if the insertion was successful, false otherwise.</returns>
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


        /// <summary>
        /// Updates the user with the given account object.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>The number of rows manipulated, 0 if nothing was manipulated.</returns>
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
                    AddAccountParameterValues(account, cmd.Parameters);

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
        private static void AddAccountParameterValues(Account account, SqliteParameterCollection parameters)
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


        /// <summary>
        /// Updates the user with the given ID with the given values.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <param name="role"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="introNarrative"></param>
        /// <param name="mainProfileID"></param>
        /// <returns>The number of rows manipulated, 0 if nothing was changed.</returns>
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


        /// <summary>
        /// Saves the user to the database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <param name="role"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="introNarrative"></param>
        /// <param name="mainProfileID"></param>
        /// <returns>True if the user was saved, false otherwise.</returns>
        public static bool SaveUser(int id, string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
        {
            if (!ExistsUser(id))
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID) >= 0;
        }


        /// <summary>
        /// Saves the user to the database.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="role"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="introNarrative"></param>
        /// <param name="mainProfileID"></param>
        /// <returns>The ID of the user saved, -1 otherwise.</returns>
        public static int SaveUser(string userID, int role, string name, string email, long phone, string address, string introNarrative, int mainProfileID)
        {
            int id = ExistsUser(userID);
            if (id < 1)
                return InsertUser(userID, role, name, email, phone, address, introNarrative, mainProfileID);
            return UpdateUser(id, userID, role, name, email, phone, address, introNarrative, mainProfileID);
        }


        /// <summary>
        /// Saves the user to the database.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        public static bool SaveUser(Account account)
        {
            if (account.IsReturning)
                return SaveUser(account.RecordID, account.GetUsername(), account.Role, account.Name, account.EmailAddress, account.PhoneNumber, account.StreetAddress, account.IntroNarrative, account.MainProfileID);
            account.RecordID = SaveUser(account.GetUsername(), account.Role, account.Name, account.EmailAddress, account.PhoneNumber, account.StreetAddress, account.IntroNarrative, account.MainProfileID);
            account.IsReturning = true;
            return account.RecordID >= 0;
        }


        /// <summary>
        /// Checks if the user exists based on their ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if the user exists, false otherwise.</returns>
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


        /// <summary>
        /// Checks if the users exists by their user hash.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>The ID of the user if they exist, -1 if they do not.</returns>
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


        /// <summary>
        /// Loads a new Account object into the SessionData provided, if the user exists.
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


        /// <summary>
        /// Loads an account from the database, if it exists.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>The a new Account object if it exists, null otherwise.</returns>
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

                        //account was not found.
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


        /// <summary>
        /// Searches the database for colleagues that contain data matching the keyphrase.
        /// </summary>
        /// <param name="keyphrase"></param>
        /// <returns>An array of IDs that meet the criteria.</returns>
        public static int[] SearchKeyphraseColleagues(string keyphrase)
        {
            if (keyphrase == null)
                return new int[0];

            List<int> ids = new List<int>();

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT id, name, email, phone, address, intro_narrative
                                FROM colleagues
                                WHERE (name LIKE @keyphrase OR email LIKE @keyphrase OR address LIKE @keyphrase OR intro_narrative LIKE @keyphrase);";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(keyphrase));

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ids.Add(GetInt32(r, 0));
                        }
                    }
                }
            }

            return ids.ToArray();
        }



        /// <summary>
        /// Loads an Account's data by their ID.
        /// </summary>
        /// <remarks>This method is intended to be used by admins.</remarks>
        /// <param name="id"></param>
        /// <returns></returns>
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
        #endregion


        #region Skill Data

        /// <summary>
        /// Saves the skill record to the database, and returns the id of the record.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns>The ID of the skill record saved.</returns>
        private static int SaveSkill(SkillData skill)
        {
            return SaveSkill(skill.SkillName);
        }


        /// <summary>
        /// Saves the skill name to the database, and returns the id of the record.
        /// </summary>
        /// <param name="skillName"></param>
        /// <returns>The ID of the skill saved.</returns>
        private static int SaveSkill(string skillName)
        {
            int id = GetSkillID(skillName);

            if (id == -1)
                return InsertSkill(skillName);

            return id;
        }


        /// <summary>
        /// Saves the skill records to the database, and returns the ids of the records.
        /// </summary>
        /// <param name="skills"></param>
        /// <returns>An array of IDs associated with the skill data records.</returns>
        private static int[] SaveSkills(params SkillData[] skills)
        {
            string[] skillNames = new string[skills.Length];
            for (int i = 0; i < skillNames.Length; i++)
                skillNames[i] = skills[i].SkillName;
            return SaveSkills(skillNames);
        }


        /// <summary>
        /// Saves the skill names to the database, and returns the ids of the records.
        /// </summary>
        /// <param name="skillNames"></param>
        /// <returns>An array of IDs associated with the skill names.</returns>
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


        /// <summary>
        /// Retrieves the IDs for the skill names if they exists.
        /// </summary>
        /// <param name="skillNames"></param>
        /// <returns>An array of IDs associated with the skill names. -1 will be in the array if a skillname was not found.</returns>
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


        /// <summary>
        /// Saves all the colleague's skills to the database.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>True if the save was successful, false otherwise.</returns>
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


        /// <summary>
        /// Saves a colleage's skills to the database.
        /// </summary>
        /// <param name="skills"></param>
        /// <returns>True if the save was successful, false otherwise.</returns>
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


        /// <summary>
        /// Saves a colleague's skill to the database.
        /// </summary>
        /// <param name="sd"></param>
        /// <returns>True if the save was successful, false otherwise.</returns>
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


        /// <summary>
        /// Searches the database for colleague skills that match the keyphrase.
        /// </summary>
        /// <param name="keyphrase"></param>
        /// <returns>An array of colleague skill IDs.</returns>
        public static int[] SearchKeyphraseSkills(string keyphrase)
        {
            if (keyphrase == null)
                return new int[0];

            List<int> ids = new List<int>();

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT cs.id, cs.colleague_id, sc.name AS skill_category, cs.skill_category_id, s.name AS skill, cs.skill_id, cs.rating
                                FROM colleague_skills cs
                                JOIN skills s ON cs.skill_id = s.id
                                LEFT JOIN skill_categories sc ON cs.skill_category_id = sc.id
                                WHERE (sc.name LIKE @keyphrase OR s.name LIKE @keyphrase);";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(keyphrase));

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ids.Add(GetInt32(r, 0));
                        }
                    }
                }
            }

            return ids.ToArray();
        }
        #endregion


        #region Education Data

        /// <summary>
        /// Inserts a new education history record into the database.
        /// </summary>
        /// <param name="colleagueID"></param>
        /// <param name="educationTypeID"></param>
        /// <param name="institutionID"></param>
        /// <param name="municipalityID"></param>
        /// <param name="stateID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="description"></param>
        /// <returns>The ID of the education history that was just inserted.</returns>
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



        /// <summary>
        /// Updates a colleagues education history record.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="colleagueID"></param>
        /// <param name="educationTypeID"></param>
        /// <param name="institutionID"></param>
        /// <param name="municipalityID"></param>
        /// <param name="stateID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="description"></param>
        /// <returns>The number of records that were affected.</returns>
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


        /// <summary>
        /// Updates the education history record with the data from the EducationData object.
        /// </summary>
        /// <param name="ed"></param>
        /// <returns>The ID of the education history record.</returns>
        public static int UpdateEducationHistory(EducationData ed)
        {
            return UpdateEducationHistory(ed.RecordID, ed.Owner.RecordID, ed.EducationTypeID, ed.InstitutionID, ed.Location.MunicipalityID, ed.Location.StateID, ed.StartDate, ed.EndDate, ed.Description);
        }


        /// <summary>
        /// Saves the education history record. If the record does not exist, it will be created. If it does exist, it will be updated.
        /// </summary>
        /// <param name="ed"></param>
        /// <returns>True if the save was successful, false otherwise.</returns>
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

        /// <summary>
        /// Gets the education type ID for the given education type.
        /// </summary>
        /// <param name="educationType"></param>
        /// <returns>The ID of the education type, -1 if it wasn't found.</returns>
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


        /// <summary>
        /// Inserts the education type into the database.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The ID of the education type that was inserted.</returns>
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


        /// <summary>
        /// Saves the education type. If the education type does not exist, it will be created. If it does exist, it will be updated.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The ID of the education type inserted, -1 if it was not inserted.</returns>
        public static int SaveEducationType(string type)
        {
            int id = GetEducationTypeID(type);

            if (id < 0)
                return InsertEducationType(type);

            return id;
        }
        #endregion


        #region Institutions

        /// <summary>
        /// Gets the institution ID for the given institution name.
        /// </summary>
        /// <param name="institutionName"></param>
        /// <returns>The ID of the institution inserted, -1 if it was not found.</returns>
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


        /// <summary>
        /// Inserts the institution into the database.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The ID of the institution that was inserted, -1 if it was not inserted.</returns>
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


        /// <summary>
        /// Saves the institution. If the institution does not exist, it will be created. If it does exist, it will be updated.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The ID of the institution being saved.</returns>
        public static int SaveInstitution(string name)
        {
            int id = GetInstitutionID(name);

            if (id < 0)
                return InsertInstitution(name);

            return id;
        }
        #endregion


        /// <summary>
        /// Checks if the education history record exists in the database.
        /// </summary>
        /// <param name="id">The ID of the education history record</param>
        /// <returns>True if the record exists, false otherwise.</returns>
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





        /// <summary>
        /// Loads the education history of a given colleague from the database and populates a dictionary with the data.
        /// </summary>
        /// <param name="account">The account of the colleague whose education history should be loaded.</param>
        /// <param name="dict">The dictionary that should be populated with the education history data.</param>
        /// <param name="useCache">Specifies whether cached data should be used, if available.</param>
        public static bool LoadColleagueEducationHistory(Account account, out Dictionary<int, EducationData> dict, bool useCache = false)
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
                    }
                }
            }

            //Console.WriteLine($"Found Education Records: {list?.Count}");

            return true;
        }


        /// <summary>
        /// Searches for a keyphrase in the education history of all colleagues.
        /// </summary>
        /// <param name="keyphrase">The keyphrase to search for</param>
        /// <returns>The IDs of education histories that contain the keyphrase.</returns>
        public static int[] SearchKeyphraseEducationHistory(string keyphrase)
        {
            if (keyphrase == null)
                return new int[0];

            List<int> ids = new List<int>();

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT eh.id, eh.colleague_id, eh.education_type_id, eh.institution_id, eh.municipality_id, eh.state_id, eh.start_date, eh.end_date, eh.description, et.type AS education_type, i.name AS institution
                                FROM education_history eh
                                JOIN education_types et ON eh.education_type_id = et.id
                                JOIN institutions i ON eh.institution_id = i.id
                                WHERE (eh.description LIKE @keyphrase OR et.type LIKE @keyphrase OR i.name LIKE @keyphrase);";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(keyphrase));

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ids.Add(GetInt32(r, 0));
                        }
                    }
                }
            }

            return ids.ToArray();
        }
        #endregion


        #region Work Data


        /// <summary>
        /// Loads the work history of a given colleague from the database and populates a list with the data.
        /// </summary>
        /// <param name="colleagueID"></param>
        /// <param name="employerID"></param>
        /// <param name="jobTitleID"></param>
        /// <param name="municipalityID"></param>
        /// <param name="stateID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="description"></param>
        /// <returns>The ID of the inserted work history record, -1 if it was not inserted.</returns>
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


        /// <summary>
        /// Inserts the work history of a given colleague to the database.
        /// </summary>
        /// <param name="wd">The WorkData object to insert.</param>
        /// <returns>The ID of the inserted work history record, -1 if it was not inserted.</returns>
        public static int InsertWorkHistory(WorkData wd)
        {
            return InsertWorkHistory(wd.Owner.RecordID, wd.EmployerID, wd.JobTitleID, wd.Location.MunicipalityID, wd.Location.StateID, wd.StartDate, wd.EndDate, wd.Description);
        }


        /// <summary>
        /// Updates the work history of a given colleague to the database.
        /// </summary>
        /// <param name="colleagueID"></param>
        /// <param name="employerID"></param>
        /// <param name="jobTitleID"></param>
        /// <param name="municipalityID"></param>
        /// <param name="stateID"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="description"></param>
        /// <returns>The rows updated in the work history table, 0 if nothing ws updated.</returns>
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


        /// <summary>
        /// Updates the work history of a given colleague to the database.
        /// </summary>
        /// <param name="wd"></param>
        /// <returns>The rows updated in the work history table, 0 if nothing ws updated.</returns>
        public static int UpdateWorkHistory(WorkData wd)
        {
            return UpdateWorkHistory(wd.RecordID, wd.Owner.RecordID, wd.EmployerID, wd.JobTitleID, wd.Location.MunicipalityID, wd.Location.StateID, wd.StartDate, wd.EndDate, wd.Description);
        }


        /// <summary>
        /// Checks if a work history record exists in the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if the work history record exists, false otherwise.</returns>
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


        /// <summary>
        /// Saves the work history of a given colleague to the database.
        /// </summary>
        /// <param name="wd"></param>
        /// <returns>True if the work history was saved, false otherwise.</returns>
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

        /// <summary>
        /// Gets the ID of an employer from the database.
        /// </summary>
        /// <param name="employer"></param>
        /// <returns>The ID of the employer, -1 if the employer was not found.</returns>
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


        /// <summary>
        /// Gets the name of an employer from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The name of the employer associated with the ID, null otherwise.</returns>
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
                    //Console.WriteLine($"Inserting employer: {name}");

                    object? employerID = cmd.ExecuteScalar();

                    if (employerID == null)
                        return -1;

                    //Console.WriteLine($"Inserted employer: id:{Convert.ToInt32(employerID)}, {name}");

                    return Convert.ToInt32(employerID);//return record ID
                }
            }

        }



        /// <summary>
        /// Saves the employer to the database.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The ID of the employer that was saved.</returns>
        public static int SaveEmployer(string name)
        {
            int id = GetEmployerID(name);

            if (id < 0)
                return InsertEmployer(name);

            return id;
        }
        #endregion


        #region Job Title

        /// <summary>
        /// Gets the ID of a job title from the database.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>The ID of the job title.</returns>
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


        /// <summary>
        /// Inserts a job title into the database.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>The ID of the job title that was just inserted.</returns>
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


        /// <summary>
        /// Saves a job title to the database.
        /// </summary>
        /// <param name="title">The name of the job title.</param>
        public static int SaveJobTitle(string title)
        {
            int id = GetJobTitleID(title);

            if (id < 0)
                return InsertJobTitle(title);

            return id;
        }
        #endregion



        /// <summary>
        /// Loads the work history of a given colleague from the database and populates a dictionary with the data.
        /// </summary>
        /// <param name="account">The account of the colleague whose work history should be loaded.</param>
        /// <param name="dict">The dictionary that should be populated with the work history data.</param>
        /// <param name="useCache">Specifies whether cached data should be used, if available.</param>
        public static bool LoadColleagueWorkHistory(Account account, out Dictionary<int, WorkData> dict, bool useCache = false)
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
            return true;
        }


        /// <summary>
        /// Searches the work history table for records that match the specified keyphrase.
        /// </summary>
        /// <param name="keyphrase">The keyphrase to search for.</param>
        /// <returns>An array of integers representing the IDs of the matching work history records.</returns>
        public static int[] SearchKeyphraseWorkHistory(string keyphrase)
        {
            if (keyphrase == null)
                return new int[0];

            List<int> ids = new List<int>();

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT wh.id, wh.colleague_id, wh.employer_id, wh.job_title_id, wh.municipality_id, wh.state_id, wh.start_date, wh.end_date, wh.description, e.name AS employer_name, jt.title AS job_title_name
                                FROM work_history wh
                                JOIN employers e ON wh.employer_id = e.id
                                JOIN job_titles jt ON wh.job_title_id = jt.id
                                WHERE (wh.description LIKE @keyphrase OR e.name LIKE @keyphrase OR jt.title LIKE @keyphrase);";

                using (SqliteCommand cmd = new(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(keyphrase));

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ids.Add(GetInt32(r, 0));
                        }
                    }
                }
            }

            return ids.ToArray();
        }
        #endregion




        #region Profile Methods

        /// <summary>
        /// Saves the provided profile data to the database. If the profile is marked for removal,
        /// the corresponding database record will be deleted. Otherwise, the profile will be updated 
        /// if it already exists in the database or inserted if it does not.
        /// </summary>
        /// <param name="pd">The profile data to be saved.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
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

                        string sql = @"INSERT INTO profiles (colleague_id, title, about_section, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering) 
                                    VALUES (@colleague_id, @title, @about_section, @colleague_skills_ids, @education_history_ids, @colleague_certs_ids, @work_history_ids, @ordering)
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

                            string aboutSection = $"{pd.ShowName}|{pd.ShowEmailAddress}|{pd.ShowPhoneNumber}|{pd.ShowIntroNarrative}";
                            cmd.Parameters.AddWithValue("@id", ValueCleaner(pd.RecordID));
                            cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(pd.Owner.RecordID));
                            cmd.Parameters.AddWithValue("@title", ValueCleaner(pd.Title));
                            cmd.Parameters.AddWithValue("@about_section", ValueCleaner(aboutSection));
#if DEBUG
                            Console.WriteLine(aboutSection);
#endif
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

        /// <summary>
        /// Updates a profile record in the database with the specified ProfileData object
        /// </summary>
        /// <param name="pd">The ProfileData object representing the updated profile</param>
        /// <returns>Returns true if the update was successful, false otherwise</returns>
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

                        string sql = @"UPDATE profiles SET colleague_id=@colleague_id, title=@title, about_section=@about_section, colleague_skills_ids=@colleague_skills_ids, education_history_ids=@education_history_ids, colleague_certs_ids=@colleague_certs_ids, work_history_ids=@work_history_ids, ordering=@ordering 
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
                            string aboutSection = $"{pd.ShowName}|{pd.ShowEmailAddress}|{pd.ShowPhoneNumber}|{pd.ShowIntroNarrative}";
                            cmd.Parameters.AddWithValue("@id", ValueCleaner(pd.RecordID));
                            cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(pd.Owner.RecordID));
                            cmd.Parameters.AddWithValue("@title", ValueCleaner(pd.Title));
                            cmd.Parameters.AddWithValue("@about_section", ValueCleaner(aboutSection));
#if DEBUG
                            Console.WriteLine(aboutSection);
#endif
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


        /// <summary>
        /// Loads all profiles associated with a given account.
        /// </summary>
        /// <param name="account">The account to load profiles for.</param>
        /// <returns>True if profiles are successfully loaded, false otherwise.</returns>
        public static bool LoadColleageProfiles(Account account)
        {
            if (account == null || account.RecordID < 0)
                return false;

            //create work history list
            List<ProfileData> list;

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                string sql = @"SELECT id, colleague_id, title, about_section, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering
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
                            skillIDs = GetString(r, 4);
                            educationIDs = GetString(r, 5);
                            certIDs = GetString(r, 6);
                            workIDs = GetString(r, 7);

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
                                GetString(r, 8) //populate ordering
                                ));


                            string[] aboutVals = GetString(r, 3).Split('|');

                            pd.ShowName = aboutVals[0].Equals("True");
                            pd.ShowEmailAddress = aboutVals[1].Equals("True");
                            pd.ShowPhoneNumber = aboutVals[2].Equals("True");
                            pd.ShowIntroNarrative = aboutVals[3].Equals("True");
#if DEBUG
                            string aboutSectionPD = $"PD: {pd.ShowName}|{pd.ShowEmailAddress}|{pd.ShowPhoneNumber}|{pd.ShowIntroNarrative}";
                            Console.WriteLine(aboutSectionPD);
#endif
                        }

                        account.Profiles = list;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Loads all profiles associated with a given account.
        /// </summary>
        /// <param name="account">The account to load profiles for.</param>
        /// <param name="dict">The dictionary to populate with profile data.</param>
        /// <returns>True if profiles are successfully loaded, false otherwise.</returns>
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

                string sql = @"SELECT id, colleague_id, title, about_section, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering
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
                            skillIDs = GetString(r, 4);
                            educationIDs = GetString(r, 5);
                            certIDs = GetString(r, 6);
                            workIDs = GetString(r, 7);

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
                                GetString(r, 8) //populate ordering
                                ));

                            string aboutSection = GetString(r, 3);
#if DEBUG
                            Console.WriteLine(aboutSection);
#endif
                            string[] aboutVals = aboutSection.Split('|');

                            pd.ShowName = aboutVals[0].Equals("True");
                            pd.ShowEmailAddress = aboutVals[1].Equals("True");
                            pd.ShowPhoneNumber = aboutVals[2].Equals("True");
                            pd.ShowIntroNarrative = aboutVals[3].Equals("True");
#if DEBUG
                            string aboutSectionPD = $"PD: {pd.ShowName}|{pd.ShowEmailAddress}|{pd.ShowPhoneNumber}|{pd.ShowIntroNarrative}";
                            Console.WriteLine(aboutSectionPD);
#endif
                            dict.TryAdd(pd.RecordID, pd);
                        }

                        account.Profiles = list;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Saves a profile data into the database. If Remove property is set, the corresponding record will be deleted. 
        /// If the RecordID property is greater than 0, the record will be updated, otherwise it will be inserted into the database.
        /// </summary>
        /// <param name="pd">ProfileData object to be saved</param>
        /// <returns>Returns true if the profile data was successfully saved or removed, false otherwise</returns>
        public static bool SaveProfile(ProfileData pd)
        {
            if (pd.Remove)
            {
                pd.IsRemoved = DeleteRecord("profiles", pd.RecordID);
                return pd.IsRemoved;
            }

            if (pd.RecordID > 0)
                return UpdateProfile(pd);

            return (pd.RecordID = InsertProfile(pd)) >= 0;
        }


        /// <summary>
        /// Searches for profile IDs that contain the given keyphrase in their title, or in their colleague_skills_ids,
        /// education_history_ids, colleague_certs_ids, or work_history_ids field. Returns an array of IDs that match the search criteria.
        /// </summary>
        /// <param name="keyphrase">The keyphrase to search for</param>
        /// <returns>An array of profile IDs that match the search criteria</returns>
        public static int[] SearchKeyphraseProfiles(string keyphrase)
        {

            if (keyphrase == null)
                return new int[0];

            ProfileResultSet resultSet = new ProfileResultSet();
            List<int> ids = new List<int>();

            using (SqliteConnection conn = new(connStr))
            {
                conn.Open();

                //would eventually need a max limit to selection size
                //only select in batches
                //or exclude ids and continue search until max size is met, then batch size after
                string sql = @"SELECT id, colleague_id, title, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids
                                FROM profiles;";

                //first load all profiles
                using (SqliteCommand cmd = new(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        ProfileResult result;
                        while (r.Read())
                        {
                            resultSet.AddResult(
                                result = new ProfileResult(
                                    GetInt32(r, 0),
                                    GetInt32(r, 1),
                                    GetString(r, 2),
                                    GetString(r, 3),
                                    GetString(r, 4),
                                    GetString(r, 5),
                                    GetString(r, 6)
                                    )
                                );

                            //check the title while we're here.
                            if (result.Title != null && result.Title.Contains(keyphrase))
                                ids.Add(result.RecordID);
                        }
                    }
                }

                int[] colleagueIDs = SearchKeyphraseColleagues(keyphrase);
                int[] skillIDs = SearchKeyphraseSkills(keyphrase);
                int[] educationIDs = SearchKeyphraseEducationHistory(keyphrase);
                int[] certifcationIDs = SearchKeyphraseCertifications(keyphrase);
                int[] workIDs = SearchKeyphraseWorkHistory(keyphrase);

                foreach (ProfileResult p in resultSet.Results)
                {

                    foreach (int i in skillIDs)
                    {
                        //match found, add profile to list and skip everything else
                        if (p.SkillRecordIDs.Contains(i))
                        {
                            ids.Add(p.RecordID);
                            goto nextLoop; // because we want to break out of this loop and continue the outer loop
                        }

                    }

                    foreach (int i in educationIDs)
                    {
                        //match found, add profile to list and skip everything else
                        if (p.EducationRecordIDs.Contains(i))
                        {
                            ids.Add(p.RecordID);
                            goto nextLoop; // because we want to break out of this loop and continue the outer loop
                        }

                    }

                    foreach (int i in certifcationIDs)
                    {
                        //match found, add profile to list and skip everything else
                        if (p.CertificationRecordIDs.Contains(i))
                        {
                            ids.Add(p.RecordID);
                            goto nextLoop; // because we want to break out of this loop and continue the outer loop
                        }

                    }

                    foreach (int i in workIDs)
                    {
                        //match found, add profile to list and skip everything else
                        if (p.WorkRecordIDs.Contains(i))
                        {
                            ids.Add(p.RecordID);
                            goto nextLoop; // because we want to break out of this loop and continue the outer loop
                        }

                    }

                //warning, goto statement
                nextLoop:
                    continue;

                }


            }

            return ids.ToArray();
        }


        /// <summary>
        /// Represents the result set of a search query for profiles.
        /// </summary>
        private class ProfileResultSet
        {
            public List<ProfileResult> Results;
            public Dictionary<int, HashSet<int>> SkillIDToProfileID;
            public Dictionary<int, HashSet<int>> EducationIDToProfileID;
            public Dictionary<int, HashSet<int>> CertificationIDToProfileID;
            public Dictionary<int, HashSet<int>> WorkIDToProfileID;


            /// <summary>
            /// Initializes a new instance of the <see cref="ProfileResultSet"/> class.
            /// </summary>
            public ProfileResultSet()
            {
                Results = new List<ProfileResult>();
                SkillIDToProfileID = new Dictionary<int, HashSet<int>>();
                EducationIDToProfileID = new Dictionary<int, HashSet<int>>();
                CertificationIDToProfileID = new Dictionary<int, HashSet<int>>();
                WorkIDToProfileID = new Dictionary<int, HashSet<int>>();
            }


            /// <summary>
            /// Adds a search result to the set of search results.
            /// </summary>
            /// <param name="result">The search result to be added.</param>
            public void AddResult(ProfileResult result)
            {
                Results.Add(result);

                AddToDictionary(result.RecordID, result.SkillRecordIDs, SkillIDToProfileID);
                AddToDictionary(result.RecordID, result.EducationRecordIDs, EducationIDToProfileID);
                AddToDictionary(result.RecordID, result.CertificationRecordIDs, CertificationIDToProfileID);
                AddToDictionary(result.RecordID, result.WorkRecordIDs, WorkIDToProfileID);
            }


            /// <summary>
            /// Adds a record ID and its corresponding set of record IDs to the specified dictionary.
            /// </summary>
            /// <param name="recordID">The record ID to be added to the dictionary.</param>
            /// <param name="recordIDs">The set of record IDs to be added to the dictionary.</param>
            /// <param name="dict">The dictionary to which the record ID and set of record IDs should be added.</param>
            private void AddToDictionary(int recordID, HashSet<int> recordIDs, Dictionary<int, HashSet<int>> dict)
            {
                HashSet<int> profileIDs;
                foreach (int i in recordIDs)
                {
                    if (dict.TryGetValue(i, out profileIDs))
                    {
                        profileIDs.Add(recordID);
                    }
                    else
                    {
                        profileIDs = new HashSet<int>() { recordID };
                        dict.Add(i, profileIDs);
                    }
                }
            }
        }



        /// <summary>
        /// Represents a single profile search result.
        /// </summary>
        private class ProfileResult
        {
            public int RecordID, ColleagueID;

            public HashSet<int> SkillRecordIDs, EducationRecordIDs, CertificationRecordIDs, WorkRecordIDs;

            public string Title;

            public string FullName;

            public string IntroNarative;

            public string EmailAddress;

            public string PhoneNumber;


            /// <summary>
            /// Initializes a new instance of the <see cref="ProfileResult"/> class.
            /// </summary>
            /// <param name="profileID">The ID of the profile.</param>
            /// <param name="colleagueID">The ID of the colleague.</param>
            /// <param name="title">The title of the profile.</param>
            /// <param name="skillIDsCsv">The comma-separated string of skill record IDs associated with the profile.</param>
            /// <param name="eduIDsCsv">The comma-separated string of education record IDs associated with the profile.</param>
            /// <param name="certIDsCsv">The comma-separated string of certification record IDs associated with the profile.</param>
            /// <param name="workIDsCsv">The comma-separated string of work experience record IDs associated with the profile.</param>
            public ProfileResult(int profileID, int colleagueID, string title, string skillIDsCsv, string eduIDsCsv, string certIDsCsv, string workIDsCsv)
            {
                RecordID = profileID;
                Title = title;
                ColleagueID = colleagueID;
                SkillRecordIDs = CreateHashSet(skillIDsCsv);
                EducationRecordIDs = CreateHashSet(eduIDsCsv);
                CertificationRecordIDs = CreateHashSet(certIDsCsv);
                WorkRecordIDs = CreateHashSet(workIDsCsv);
            }


            /// <summary>
            /// Creates a hash set of integers from the given comma-separated string of values.
            /// </summary>
            /// <param name="values">The comma-separated string of values.</param>
            /// <returns>A hash set of integers.</returns>
            HashSet<int> CreateHashSet(string values)
            {
                if (string.IsNullOrEmpty(values))
                    return new HashSet<int>();
                else if (values.Length == 1)
                    return new HashSet<int>() { int.Parse(values) };
                else
                    return new HashSet<int>(values.Split(',').Select(int.Parse));
            }

        }
        #endregion



        #region Certification methods

        /// <summary>
        /// Saves a certification type to the database, inserting it if it doesn't exist already.
        /// </summary>
        /// <param name="certificationType">The name of the certification type to save.</param>
        /// <returns>The ID of the certification type in the database.</returns>
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

        /// <summary>
        /// Inserts a new certification into the database.
        /// </summary>
        /// <param name="cd">The certification data to insert.</param>
        /// <returns>The ID of the new certification in the database.</returns>
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

                    object? id = cmd.ExecuteScalar();

                    if (id == null)
                        return -1;

                    return Convert.ToInt32(id);//return record ID
                }
            }
        }


        /// <summary>
        /// Updates an existing certification in the database.
        /// </summary>
        /// <param name="cd">The certification data to update.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
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


        /// <summary>
        /// Saves a CertificationData object to the database.
        /// </summary>
        /// <param name="cd">The CertificationData object to save.</param>
        /// <returns>True if the save operation succeeded, otherwise false.</returns>
        public static bool SaveCertification(CertificationData cd)
        {

            if (cd.Remove)
            {
                //delete cert
                return cd.IsRemoved = DeleteRecord("colleague_certs", cd.RecordID);
            }

            cd.CertificationTypeID = SaveCertificationType(cd.CertificationType);
            cd.InstitutionID = SaveInstitution(cd.Institution);

            if (cd.RecordID > 0)
                return UpdateCertification(cd);
            return (cd.RecordID = InsertCertification(cd)) >= 0;
        }


        /// <summary>
        /// Loads the CertificationData objects for a given account from the database into a dictionary.
        /// </summary>
        /// <param name="account">The account to load the certifications for.</param>
        /// <param name="dict">The dictionary to load the certifications into.</param>
        /// <returns>True if the load operation succeeded, otherwise false.</returns>
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

            return true;
        }


        /// <summary>
        /// Searches the database for CertificationData objects that match a given keyphrase.
        /// </summary>
        /// <param name="keyphrase">The keyphrase to search for.</param>
        /// <returns>An array of integers representing the IDs of the matching CertificationData objects.</returns>
        public static int[] SearchKeyphraseCertifications(string keyphrase)
        {

            if (keyphrase == null)
                return new int[0];

            List<int> ids = new List<int>();

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();


                string sql = @"SELECT cc.id, cc.colleague_id, cc.cert_type_id, cc.institution_id, cc.municipality_id, cc.state_id, cc.start_date, cc.end_date, cc.description, ct.type AS cert_type, i.name AS institution
                                            FROM colleague_certs cc
                                            JOIN cert_types ct ON cc.cert_type_id = ct.id
                                            JOIN institutions i ON cc.institution_id = i.id
                                            WHERE (cc.description LIKE @keyphrase OR ct.type LIKE @keyphrase OR i.name LIKE @keyphrase);";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(keyphrase));

                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {

                        //get all matching ids
                        while (r.Read())
                            ids.Add(GetInt32(r, 0));

                    }
                }
            }

            return ids.ToArray();
        }
        #endregion



        #region Debugging

        /// <summary>
        /// Retrieves all records from the specified table and returns a list of strings representing each record.
        /// </summary>
        /// <param name="table">The name of the table to retrieve records from.</param>
        /// <returns>A list of strings representing each record.</returns>
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
                account.AddSkills($"Other", $"TestSkill{s}");
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


        //created by ChatGPT (based on my Thomas method)
        public static void Andrew(Account account)
        {
            // Replace this with a unique token for Andrew's account
            // example: qw9FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k
            account.UpdateData(
                "Roe, Andrew Chadwick",
                "andrew.johnson@example.com",
                1234567890,
                @"Passionate software developer with experience in creating modern web applications and APIs. Proficient in various programming languages and frameworks. Seeking new opportunities to leverage skills and contribute to innovative projects."
            );

            account.AddEducation(
                "Stanford University",
                "Bachelor of Science",
                "Computer Science",
                null,
                Utilities.ToDateOnly("September 1 2015"),
                Utilities.ToDateOnly("June 15 2019")
            );

            account.AddEducation(
                "Augusta University",
                "Master of Science",
                "Information Security Management",
                null,
                Utilities.ToDateOnly("September 1 2019"),
                Utilities.ToDateOnly("June 15 2021")
            );

            account.AddEducation(
                "Georgia Regents University",
                "Bachelor of Business Administration",
                "Management Information Systems",
                null,
                Utilities.ToDateOnly("September 1 2014"),
                Utilities.ToDateOnly("June 15 2018")
            );

            account.AddEducation(
                "Columbia University",
                "Master of Science",
                "Data Science",
                null,
                Utilities.ToDateOnly("September 1 2016"),
                Utilities.ToDateOnly("June 15 2018")
            );

            account.AddEducation(
                "University of Pennsylvania",
                "Bachelor of Applied Science",
                "Computer and Cognitive Science",
                null,
                Utilities.ToDateOnly("September 1 2013"),
                Utilities.ToDateOnly("June 15 2017")
            );

            account.AddEducation(
                "University of Michigan",
                "Master of Science",
                "Computer Science and Engineering",
                null,
                Utilities.ToDateOnly("September 1 2015"),
                Utilities.ToDateOnly("June 15 2017")
            );


            account.AddWork(
                "Innovative Tech Solutions",
                "Software Engineer",
                "Developed web applications using .NET Core and Angular. Collaborated with cross-functional teams to deliver high-quality software products.",
                null,
                Utilities.ToDateOnly("July 1 2019"),
                Utilities.ToDateOnly("March 31 2023")
            );

            account.AddWork(
                "Global Software Inc.",
                "Senior Software Engineer",
                "Designed and developed scalable microservices using Java, Spring Boot, and Kubernetes. Implemented CI/CD pipelines for efficient software deployment.",
                null,
                Utilities.ToDateOnly("April 1 2023"),
                Utilities.ToDateOnly("April 1 2026")
            );

            account.AddWork(
                "Cutting Edge Solutions",
                "Full Stack Developer",
                "Built responsive web applications using React, Redux, and Node.js. Worked closely with designers and product managers to meet user requirements.",
                null,
                Utilities.ToDateOnly("July 1 2016"),
                Utilities.ToDateOnly("June 30 2019")
            );

            account.AddWork(
                "AI Innovations",
                "Machine Learning Engineer",
                "Developed and deployed machine learning models for predictive analytics and recommendation systems. Utilized Python, TensorFlow, and Scikit-learn.",
                null,
                Utilities.ToDateOnly("January 1 2014"),
                Utilities.ToDateOnly("June 30 2016")
            );

            account.AddWork(
                "TechStart",
                "Software Engineering Intern",
                "Collaborated with a team to build a web application using Ruby on Rails and PostgreSQL. Implemented user authentication and API integrations.",
                null,
                Utilities.ToDateOnly("June 1 2013"),
                Utilities.ToDateOnly("August 31 2013")
            );


            account.AddSkills("Programming Languages", "C#", "JavaScript", "HTML", "CSS", "Python", "Ruby", "PHP", "Swift", "TypeScript");
            account.AddSkills("OS", "Windows", "Linux", "macOS", "Ubuntu", "Debian", "Fedora", "Red Hat Enterprise Linux", "FreeBSD", "Android");
            account.AddSkills("Software and Framework", ".NET Core", "Angular", "Node.js", "Django", "Flask", "Ruby on Rails", "ASP.NET", "Spring Boot", "Laravel", "Express.js", "React", "Vue.js", "Ember.js", "Backbone.js", "Bootstrap", "Tailwind CSS");

            account.AddSkills("Other", "Git", "Agile Methodologies", "Docker", "Kubernetes", "NoSQL Databases", "SQL", "RESTful APIs", "React", "Vue.js", "Cloud Computing", "AWS", "Azure", "GCP", "CI/CD", "Jenkins", "Terraform", "Ansible", "Data Structures", "Algorithms");

            account.AddCertification("Microsoft", "Microsoft Certified Solutions Developer (MCSD)", null, null);
            account.AddCertification("Amazon Web Services", "AWS Certified Developer - Associate", null, null);
            account.AddCertification("Google Cloud", "Google Cloud Professional Developer", null, null);
            account.AddCertification("Scrum.org", "Professional Scrum Master I (PSM I)", null, null);
            account.AddCertification("Oracle", "Oracle Certified Professional, Java SE 8 Programmer", null, null);
            account.AddCertification("Cisco", "Cisco Certified Network Associate (CCNA)", null, null);

            // Save all changes
            account.PersistAll();

            // Create a profile for Andrew
            ProfileData profile = account.CreateProfile("Main");
            profile.AddWork(1);
            profile.AddWork(2);
            profile.AddWork(3);
            profile.AddWork(4);
            profile.AddEducation(1);
            profile.AddEducation(3);
            profile.AddEducation(2);

            //ummm? chatgpt actually wrote this, LOL, poor thing.
            for (int i = 1; i <= 10; i++)
            {
                profile.AddSkill(i);
            }
            for (int i = 11; i <= 30; i++)
            {
                profile.AddSkill(i);
            }


            profile = account.CreateProfile("Sub1");
            profile.AddWork(1);
            profile.AddWork(2);
            profile.AddWork(3);
            profile.AddWork(4);
            profile.AddEducation(1);
            profile.AddEducation(3);
            profile.AddEducation(2);

            //ummm? chatgpt actually wrote this, LOL, poor thing.
            for (int i = 1; i <= 10; i++)
            {
                profile.AddSkill(i);
            }
            for (int i = 11; i <= 30; i++)
            {
                profile.AddSkill(i);
            }


            profile = account.CreateProfile("Sub2");
            profile.AddWork(1);
            profile.AddWork(2);
            profile.AddWork(3);
            profile.AddWork(4);
            profile.AddEducation(1);
            profile.AddEducation(3);
            profile.AddEducation(2);

            //ummm? chatgpt actually wrote this, LOL, poor thing.
            for (int i = 1; i <= 10; i++)
            {
                profile.AddSkill(i);
            }
            for (int i = 11; i <= 30; i++)
            {
                profile.AddSkill(i);
            }

            // Save all changes
            account.PersistAll();

            Console.WriteLine(SearchKeyphraseProfiles("C#").Length);
            Console.WriteLine(SearchKeyphraseProfiles("Mandy").Length);
            Console.WriteLine(SearchKeyphraseProfiles("Augusta University").Length);
            Console.WriteLine("Saved!");
        }



        //this one was actually (almost) created by the real ChatGPT
        public static void ChatGPT(Account account)
        {
            // Replace this with a unique token for ChatGPT's account
            // example: 1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p7q8r9s0t1u2v
            account.UpdateData(
                "7.0, Sir ChatGPT",
                "chatgpt7.0@example.com",
                0987654321,
                @"Highly skilled and experienced AI engineer with expertise in natural language processing, machine learning, and data analysis. Proven track record in developing and deploying large-scale AI solutions for various applications. Seeking new opportunities to innovate and drive AI advancements."
            );

            account.AddEducation(
                "Massachusetts Institute of Technology",
                "Ph.D.",
                "Artificial Intelligence",
                null,
                Utilities.ToDateOnly("September 1 2013"),
                Utilities.ToDateOnly("June 15 2018")
            );

            account.AddEducation(
                "University of California, Berkeley",
                "Master of Science",
                "Computer Science",
                null,
                Utilities.ToDateOnly("September 1 2011"),
                Utilities.ToDateOnly("June 15 2013")
            );

            account.AddEducation(
                "Carnegie Mellon University",
                "Bachelor of Science",
                "Computer Science",
                null,
                Utilities.ToDateOnly("September 1 2007"),
                Utilities.ToDateOnly("June 15 2011")
            );

            account.AddWork(
                "OpenAI",
                "AI Research Engineer",
                "Developed state-of-the-art natural language processing models and contributed to the development of GPT series models. Worked on improving model performance, scalability, and safety.",
                null,
                Utilities.ToDateOnly("July 1 2018"),
                Utilities.ToDateOnly("March 31 2023")
            );

            account.AddWork(
                "Tech Innovations Inc.",
                "Machine Learning Engineer",
                "Designed and implemented machine learning algorithms for various applications. Optimized models for performance and efficiency. Collaborated with cross-functional teams to integrate AI solutions.",
                null,
                Utilities.ToDateOnly("July 1 2013"),
                Utilities.ToDateOnly("June 30 2018")
            );

            account.AddSkills("Programming Languages", "Python", "C++", "Java", "Scala");
            account.AddSkills("OS", "Linux", "macOS", "Windows");
            account.AddSkills("Software and Framework", "TensorFlow", "PyTorch", "Keras", "Scikit-learn");
            account.AddSkills("Other", "Natural Language Processing", "Deep Learning", "Data Analysis", "Big Data");

            account.AddCertification("Google", "TensorFlow Developer Certificate", null, null);
            account.AddCertification("NVIDIA", "Deep Learning Institute (DLI) Certificate", null, null);
            account.AddCertification("Coursera", "Deep Learning Specialization Certificate", null, null);
            account.AddCertification("Stanford University", "Natural Language Processing Certificate", null, null);

            // Save all changes
            account.PersistAll();

            // Create a profile for ChatGPT
            ProfileData profile = account.CreateProfile("Main");
            profile.AddWork(1);
            profile.AddWork(2);
            profile.AddEducation(1);
            profile.AddEducation(2);
            profile.AddEducation(3);
            profile.AddSkill(1);
            profile.AddSkill(2);
            profile.AddSkill(3);
        }



        public static TableModels TableModels;
        private static void TestTableModels()
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


        /// <summary>
        /// Inserts a list of values into a table if they don't already exist. The query is wrapped in an INSERT OR IGNORE statement.
        /// </summary>
        /// <param name="cmd">The SqliteCommand instance to execute the query.</param>
        /// <param name="tableName">The name of the table to insert the values into.</param>
        /// <param name="fieldName">The name of the column in the table to insert the values into.</param>
        /// <param name="values">The list of values to insert.</param>
        /// <returns>A list of the IDs of the inserted values.</returns>
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


        /// <summary>
        /// Deletes a record from the specified table with the given ID.
        /// </summary>
        /// <param name="cmd">The SqliteCommand instance to execute the query.</param>
        /// <param name="tableName">The name of the table to delete the record from.</param>
        /// <param name="id">The ID of the record to delete.</param>
        /// <returns>True if the record is deleted successfully, false otherwise.</returns>
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


        /// <summary>
        /// Deletes multiple records from the specified table with the given IDs.
        /// </summary>
        /// <param name="cmd">The SqliteCommand instance to execute the query.</param>
        /// <param name="tableName">The name of the table to delete the records from.</param>
        /// <param name="ids">The IDs of the records to delete.</param>
        /// <returns>True if the records are deleted successfully, false otherwise.</returns>
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


        /// <summary>
        /// Deletes a record from the specified table with the given ID.
        /// </summary>
        /// <param name="tableName">The name of the table to delete the record from.</param>
        /// <param name="id">The ID of the record to delete.</param>
        /// <returns>True if the record is deleted successfully, false otherwise.</returns>
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


        /// <summary>
        /// Deletes multiple records from the specified table with the given IDs.
        /// </summary>
        /// <param name="tableName">The name of the table to delete the records from.</param>
        /// <param name="ids">The IDs of the records to delete.</param>
        /// <returns>True if the records are deleted successfully, false otherwise.</returns>
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


        /// <summary>
        /// Cleans the value of an object by converting null values to DBNull.Value.
        /// </summary>
        /// <param name="val">The value to clean.</param>
        /// <returns>The cleaned value.</returns>
        protected static object ValueCleaner(object val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }


        /// <summary>
        /// Cleans the value of an integer by converting 0 and -1 values to DBNull.Value.
        /// </summary>
        /// <param name="val">The integer to clean.</param>
        /// <returns>The cleaned integer.</returns>
        protected static object ValueCleaner(int val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }


        /// <summary>
        /// Cleans the value of a long integer by converting 0 and -1 values to DBNull.Value.
        /// </summary>
        /// <param name="val">The long integer to clean.</param>
        /// <returns>The cleaned long integer.</returns>
        protected static object ValueCleaner(long val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }


        /// <summary>
        /// Cleans the value of a string by converting null values to DBNull.Value.
        /// </summary>
        /// <param name="val">The string to clean.</param>
        /// <returns>The cleaned string.</returns>
        protected static object ValueCleaner(string val)
        {
            if (val == null)
                return DBNull.Value;

            //Test before using.

            return val;//Utilities.HtmlStripper(val);
        }


        /// <summary>
        /// Cleans the value of a DateOnly object by converting null values to DBNull.Value.
        /// </summary>
        /// <param name="val">The DateOnly object to clean.</param>
        /// <returns>The cleaned DateOnly object.</returns>
        protected static object ValueCleaner(DateOnly? val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }


        /// <summary>
        /// Gets a long integer value from the specified ordinal in a SqliteDataReader.
        /// </summary>
        /// <param name="r">The SqliteDataReader containing the value.</param>
        /// <param name="ordinal">The ordinal of the value.</param>
        /// <returns>The long integer value.</returns>
        protected static long GetInt64(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt64(ordinal);
        }


        /// <summary>
        /// Gets an integer value from the specified ordinal in a SqliteDataReader.
        /// </summary>
        /// <param name="r">The SqliteDataReader containing the value.</param>
        /// <param name="ordinal">The ordinal of the value.</param>
        /// <returns>The integer value.</returns>
        protected static int GetInt32(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt32(ordinal);
        }


        /// <summary>
        /// Gets a string value from the specified ordinal in a SqliteDataReader.
        /// </summary>
        /// <param name="r">The SqliteDataReader containing the value.</param>
        /// <param name="ordinal">The ordinal of the value.</param>
        /// <returns>The string value.</returns>
        protected static string GetString(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return null;
            return r.GetString(ordinal);
        }


        /// <summary>
        /// Gets a DateOnly value from the specified ordinal in a SqliteDataReader.
        /// </summary>
        /// <param name="r">The SqliteDataReader containing the value.</param>
        /// <param name="ordinal">The ordinal of the value.</param>
        /// <returns>The DateOnly value.</returns>
        protected static DateOnly GetDateOnly(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return DateOnly.MinValue;
            return Utilities.ToDateOnly(r.GetDateTime(ordinal));
        }
        #endregion





        private const string _resetTablesSql = @"

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

        ";


        ///<summary>
        ///This is initial query used for creating the initial database and tables.
        ///Should only be used if the database is being completely reset or initially created.
        ///</summary>
        private const string _schemaCheckSql = @"

        CREATE TABLE colleagues (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          user_hash TEXT UNIQUE NOT NULL,
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
          about_section TEXT,           --about section of the profile
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
