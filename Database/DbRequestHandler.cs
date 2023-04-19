using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;

namespace SCCPP1.Database
{
    public class DbRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly SqliteConnection _connection;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { _isBusy = value; }
        }


        public DbRequestHandler()
        {
            _semaphore = new SemaphoreSlim(1);
            _connection = DbRequestManager.RequestConnection(this);
            _cancellationTokenSource = new CancellationTokenSource();
        }


        public async Task HandleRequest(DbRequest request)
        {
            try
            {
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                IsBusy = true;

                // Code to handle the request goes here
                // You can access the account with request.GetAccount()

                await Task.Run(() => { });

                IsBusy = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public void CloseConnection()
        {
            if (_connection != null)
                _connection.Close();
        }

        /*

        public bool InsertAccount(Account account)
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


        public int UpdateUser(Account account)
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
        private void AddParameterValues(Account account, SqliteParameterCollection parameters)
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

        public int CreateUser(SessionData data)
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
        }*/



    }
}
