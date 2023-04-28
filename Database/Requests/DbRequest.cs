using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using System.Security.AccessControl;

namespace SCCPP1.Database.Requests
{

    public abstract class DbRequest
    {
        public enum RequestStatus
        {
            PENDING,
            PROCESSING,
            COMPLETED
        }

        public RequestStatus Status { get; set; }

        private SessionData _sessionData;

        private DbRequestHandler _handler;

        private bool _isExecuting;
        public bool Success { get; internal set; }
        protected object? Result;

        protected internal DbRequest(SessionData sessionData)
        {
            _sessionData = sessionData;
            Status = RequestStatus.PENDING;

#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] new request for object: {sessionData.GetType().Name}");
#endif
        }

        public SessionData GetSessionData() => _sessionData;


        internal void SetHandler(DbRequestHandler handler)
        {
            // Only set once
            if (_handler == null)
            {
#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] handler set.");
#endif
                _handler = handler;
            }
        }

        //used as a wrapper for RunCommand so that we can set the handler;
        //this prevents this request from being processed by another handler
        internal bool Execute(DbRequestHandler handler)
        {
            if (_handler != null)
                throw new System.Exception("This request is already being processed by another handler.");
#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] executing request...");
#endif
            SetHandler(handler);
            try
            {
                ResetCommand();
                Success = RunCommand(handler.Command);
            }
            catch (SqliteException e)
            {

#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] handler set.");
                Console.WriteLine($"[{GetType().Name}] {e.Message}");
                Console.WriteLine($"[{GetType().Name}] Command: {e.BatchCommand}");
                Console.WriteLine($"[{GetType().Name}] Data: {e.Data}");
#endif
            }
            catch (Exception e)
            {
#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] {e.Message}");
#endif
            }
            finally
            {
                _isExecuting = false;
                Status = RequestStatus.COMPLETED;
#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] Executed request. Success? {Success}");
#endif
            }
            return Success;
        }


        protected internal abstract bool RunCommand(SqliteCommand cmd);



        #region Sql helper methods
        protected virtual bool Delete(SqliteCommand cmd, string tableName, int id)
        {
            cmd.CommandText = $"DELETE FROM {tableName} WHERE id={id};";
            cmd.ExecuteNonQuery();
            return true;
        }

        public int ResultAsID()
        {
            if (Result != null)
                return Convert.ToInt32(Result);
            return -1;
        }

        public object? ResultAsObject()
        {

#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] Returning result as object.");
#endif
            return Result;
        }

        protected void ResetCommand()
        {
            if (_handler != null)
                _handler.ResetCommand();
        }


        protected static object ValueCleaner(object val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }


        protected static object ValueCleaner(int val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }

        protected static object ValueCleaner(long val)
        {
            if (val == 0 || val == -1)
                return DBNull.Value;

            return val;
        }

        protected static object ValueCleaner(string val)
        {
            if (val == null)
                return DBNull.Value;

            //Test before using.

            return val;//Utilities.HtmlStripper(val);
        }

        protected static object ValueCleaner(DateOnly? val)
        {
            if (val == null)
                return DBNull.Value;

            return val;
        }

        protected static long GetInt64(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt64(ordinal);
        }

        protected static int GetInt32(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt32(ordinal);
        }

        protected static string GetString(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return null;
            return r.GetString(ordinal);
        }

        protected static DateOnly GetDateOnly(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return DateOnly.MinValue;
            return Utilities.ToDateOnly(r.GetDateTime(ordinal));
        }
        #endregion
    }
}
