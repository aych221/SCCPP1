using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using System.Data;

namespace SCCPP1.Database.Requests.Operations
{
    public abstract class RecordDataRequest : DbRequest
    {

        public RecordDataRequest(SessionData sessionData)
            : base(sessionData)
        {
        }

        public RecordDataRequest(Account account)
            : this(account.Data)
        {
        }

        public Account GetAccount() => GetSessionData().Owner;

        protected virtual bool Insert(SqliteCommand cmd, bool addOrIgnore, string tableName, string columnName, object value)
        {
            cmd.CommandText = $"INSERT {(addOrIgnore ? "OR IGNORE INTO" : "INTO")} {tableName} ({columnName}) VALUES (@val){(addOrIgnore ? " RETURNING id" : "")};";
            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));

            //We return the ID if insert is Or Ignore
            if (!addOrIgnore)
            {
                //Result = cmd.ExecuteScalar();

                try
                {
                    Result = cmd.ExecuteScalar();
                }
                catch (SqliteException e)
                {
#if DEBUG
                    Console.WriteLine($"[{GetType().Name}] handler set.");
                    Console.WriteLine($"[{GetType().Name}] {e.Message}");
                    Console.WriteLine($"[{GetType().Name}] Command: {e.BatchCommand}");
                    Console.WriteLine($"[{GetType().Name}] Data: {e.Data}");
#endif
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine($"[{GetType().Name}] {e.Message}");
#endif
                }

                if (Result == null)
                    return false;
            }
            else
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
#if DEBUG
                    Console.WriteLine($"[{GetType().Name}] handler set.");
                    Console.WriteLine($"[{GetType().Name}] {e.Message}");
                    Console.WriteLine($"[{GetType().Name}] Command: {e.BatchCommand}");
                    Console.WriteLine($"[{GetType().Name}] Data: {e.Data}");
#endif
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine($"[{GetType().Name}] {e.Message}");
#endif
                }
            return true;//return record ID
        }


        protected virtual bool Select(SqliteCommand cmd, string tableName, string columnName, int id)
        {
            cmd.CommandText = $"SELECT {columnName} FROM {tableName} WHERE id={id};";

            using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
            {
                if (r.Read())
                {
                    Result = GetString(r, 0);
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// This will Insert or Ignore a single value into a table then return the ID of the record.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual int PersistSingleValue(SqliteCommand cmd, string tableName, string columnName, object value)
        {
            //attempt to insert the value, but if it already exists, ignore it
            cmd.CommandText = $"INSERT OR IGNORE INTO {tableName} ({columnName}) VALUES (@val);";
#if DEBUG
            Console.WriteLine($"[{GetType().Name}] Command: \"{cmd.CommandText}\" Value: \"{ValueCleaner(value)}\"");
#endif
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException e)
            {
#if DEBUG
                Console.WriteLine($"[{GetType().Name}] handler set.");
                Console.WriteLine($"[{GetType().Name}] {e.Message}");
                Console.WriteLine($"[{GetType().Name}] Command: {e.BatchCommand}");
                Console.WriteLine($"[{GetType().Name}] Data: {e.Data}");
#endif
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"[{GetType().Name}] {e.Message}");
#endif
            }

            //reset command
            //ResetCommand();

            //get ID of the value
            cmd.CommandText = $"SELECT id FROM {tableName} WHERE {columnName}=@val;";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));


            using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleResult))
            {
                if (r.Read())
                    return GetInt32(r, 0);

                //reset command to be used again
                ResetCommand();
            }

            return -1;
        }
    }
}
