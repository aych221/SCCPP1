using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;

namespace SCCPP1.Database.Requests.Operations
{
    public abstract class OwnerRecordDataRequest : DbRequest
    {

        public OwnerRecordDataRequest(Account account)
            : base(account)
        {
        }

        protected virtual bool Insert(SqliteCommand cmd, bool addOrIgnore, string tableName, string columnName, object value)
        {
            cmd.CommandText = $"INSERT {(addOrIgnore ? "OR IGNORE INTO" : "INTO")} {tableName} ({columnName}) VALUES (@val){(addOrIgnore ? " RETURNING id" : "")};";
            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));

            //We return the ID if insert is Or Ignore
            if (!addOrIgnore)
            {
                Result = cmd.ExecuteScalar();

                if (Result == null)
                    return false;
            }
            else
                cmd.ExecuteNonQuery();
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


        protected virtual bool Delete(SqliteCommand cmd, string tableName, int id)
        {
            cmd.CommandText = $"DELETE FROM {tableName} WHERE id={id};";
            cmd.ExecuteNonQuery();
            return true;
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
            cmd.Parameters.AddWithValue("@val", ValueCleaner(value));

            cmd.ExecuteNonQuery();


            //reset command
            ResetCommand();

            //get ID of the value
            cmd.CommandText = $"SELECT id FROM {tableName} WHERE {columnName}=@val;";


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
