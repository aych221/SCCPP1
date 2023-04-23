using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using System.Collections.Concurrent;
using System.Data;

namespace SCCPP1.Database.Requests
{
    public class DbRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly SqliteConnection _connection;
        protected internal SqliteConnection Connection { get { return _connection; } }


        private readonly SqliteCommand _command;
        protected internal SqliteCommand Command { get { return _command; } }

        protected internal PriorityQueue<DbRequest, int> Requests { get; set; }

        internal int HandlingCount;

        private int Count => Requests.Count;
        public bool IsBusy => _semaphore.CurrentCount == 0;




        public DbRequestHandler(SqliteConnection connection)
        {
            Requests = new PriorityQueue<DbRequest, int>();

            _semaphore = new SemaphoreSlim(1);
            _cancellationTokenSource = new CancellationTokenSource();

            _connection = connection;
            _command = _connection.CreateCommand();
            ResetCommand();
        }


        ~DbRequestHandler()
        {
            //use discard to finish shutdown after garbage collector is called
            _ = ShutDownAsync();
        }

        public async Task ProcessRequestsAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                DbRequest request = null;
                await _semaphore.WaitAsync();
                try
                {
                    if (Count > 0)
                        request = NextRequest();
                }
                finally
                {
                    _semaphore.Release();
                }


                if (request != null)
                    await HandleRequestAsync(request);
                else
                    await Task.Delay(DbConstants.HANDLER_SLEEP_TIME);
            }
        }

        private DbRequest NextRequest()
        {
            return Requests.Dequeue();
        }

        internal async Task<bool> HandleRequestAsync(DbRequest request)
        {
            bool success = false;
            try
            {
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);

                //handle request if needed
                request.SetHandler(this);

                success = ProcessRequest(request);

            }
            finally
            {
                _semaphore.Release();

                if (success)
                    await CompleteRequestAsync(request);
                else
                    await FailRequestAsync(request);
            }
            return success;
        }

        private bool ProcessRequest(DbRequest request)
        {
            return request.Execute(this);
        }

        private async Task CompleteRequestAsync(DbRequest request)
        {
            //set request's PostOperationStatus flag
            await Task.Run(() => { request.Status = DbRequest.RequestStatus.COMPLETED; });
        }

        private async Task FailRequestAsync(DbRequest request)
        {
            //set request's PostOperationStatus flag
            await Task.Run(() => { request.Status = DbRequest.RequestStatus.COMPLETED; });
        }


        internal async Task<bool> CancelRequest(DbRequest request)
        {
            bool result = false;
            try
            {
                //wait for token to access request
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);

                if (Requests.TryDequeue(out request, out int priority))
                    result = true;
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }


        internal async Task<bool> ShutDownAsync()
        {
            // Set the cancellation token to request cancellation
            _cancellationTokenSource.Cancel();

            // Wait for the semaphore to be released by the ProcessRequestsAsync() method
            await _semaphore.WaitAsync();

            // Complete all remaining requests as failed
            while (Requests.Count > 0)
            {
                DbRequest request = Requests.Dequeue();
                await FailRequestAsync(request);
            }

            //release token(s) and dispose resources
            _semaphore.Release();
            _cancellationTokenSource.Dispose();
            _connection.Dispose();

            return true;
        }

        internal void CloseConnection()
        {
            if (_connection != null)
                _connection.Close();
        }

        internal void OpenConnection()
        {
            if (_connection != null)
                _connection.Open();
        }

        internal void DisposeConnection()
        {
            if (_connection != null)
                _connection.Dispose();
        }


        internal void ResetCommand()
        {
            //clear command text and params
            Command.CommandText = string.Empty;
            Command.Parameters.Clear();

            //set properties to defaults
            Command.CommandType = CommandType.Text;
            Command.CommandTimeout = 30;
            Command.DesignTimeVisible = true;
            Command.UpdatedRowSource = UpdateRowSource.None;
        }

    }

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