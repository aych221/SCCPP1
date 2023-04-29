using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using System.Security.AccessControl;

namespace SCCPP1.Database.Requests
{

    /// <summary>
    /// Represents a database request that can be executed to perform database operations.
    /// </summary>
    public abstract class DbRequest
    {    /// <summary>
         /// The status of the database request.
         /// </summary>
        public enum RequestStatus
        {
            PENDING,    //waiting to be executed.
            PROCESSING, //currently being processed (executed).
            COMPLETED   //completed execution.
        }

        /// <summary>
        /// The <see cref="DbRequest.RequestStatus"/> of the database request.
        /// </summary>
        public RequestStatus Status { get; set; }

        /// <summary>
        /// The session data for the request.
        /// </summary>
        private SessionData _sessionData;

        /// <summary>
        /// The database request handler for the request.
        /// </summary>
        private DbRequestHandler _handler;

        /// <summary>
        /// Indicates whether the request is currently being executed.
        /// </summary>
        private bool _isExecuting;

        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// The result of the request.
        /// </summary>
        protected object? Result;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbRequest"/> class with the specified session data.
        /// </summary>
        /// <param name="sessionData">The session data for the request.</param>
        protected internal DbRequest(SessionData sessionData)
        {
            _sessionData = sessionData;
            Status = RequestStatus.PENDING;

#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] new request for object: {sessionData.GetType().Name}");
#endif
        }


        /// <summary>
        /// Gets the session data for the request.
        /// </summary>
        /// <returns>The session data for the request.</returns>
        public SessionData GetSessionData() => _sessionData;


        /// <summary>
        /// Sets the database request handler for the request.
        /// </summary>
        /// <param name="handler">The database request handler for the request.</param>
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


        /// <summary>
        /// Executes the database request with the specified database request handler.
        /// </summary>
        /// <param name="handler">The database request handler to execute the request with.</param>
        /// <returns>true if the request was executed successfully, false otherwise.</returns>
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


        /// <summary>
        /// Runs the SQLite command for this database request using the given command object.
        /// </summary>
        /// <param name="cmd">The SQLite command object to use.</param>
        /// <returns>True if the command was executed successfully, false otherwise.</returns>
        protected internal abstract bool RunCommand(SqliteCommand cmd);



        #region Sql helper methods
        /// <summary>
        /// Converts the result of this database request to an ID value.
        /// </summary>
        /// <returns>The ID value as an integer, or -1 if the result is null.</returns>
        protected virtual bool Delete(SqliteCommand cmd, string tableName, int id)
        {
            cmd.CommandText = $"DELETE FROM {tableName} WHERE id={id};";
            cmd.ExecuteNonQuery();
            return true;
        }


        /// <summary>
        /// Converts the result of this database request to an ID value.
        /// </summary>
        /// <returns>The ID value as an integer, or -1 if the result is null.</returns>
        public int ResultAsID()
        {
            if (Result != null)
                return Convert.ToInt32(Result);
            return -1;
        }


        /// <summary>
        /// Gets the result of this database request as an object.
        /// </summary>
        /// <returns>The result object.</returns>
        public object? ResultAsObject()
        {

#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] Returning result as object.");
#endif
            return Result;
        }


        /// <summary>
        /// Resets the SQLite command for this database request using the handler.
        /// </summary>
        protected void ResetCommand()
        {
            if (_handler != null)
                _handler.ResetCommand();
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
    }
}
