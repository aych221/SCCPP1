using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.User;
using System.Data;

namespace SCCPP1.Database.Requests.Operations
{
    public abstract class SingleValueRecordRequest : DbRequest
    {

        protected string TableName { get; private set; }

        protected string ColumnName { get; private set; }


        //request is meant for tables that are only have id and value columns.
        public SingleValueRecordRequest(Account account, string tableName, string columnName) : base(account)
        {
            TableName = tableName;
            ColumnName = columnName;
        }


        protected virtual bool Insert(SqliteCommand cmd, bool addOrIgnore, object value)
        {
            cmd.CommandText = $"INSERT {(addOrIgnore ? "OR IGNORE INTO" : "INTO")} {TableName} ({ColumnName}) VALUES (@val){(addOrIgnore ? " RETURNING id" : "")};";
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


        protected virtual bool Select (SqliteCommand cmd, int id)
        {
            cmd.CommandText = $"SELECT {ColumnName} FROM {TableName} WHERE id={id};";

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


        protected virtual bool Delete(SqliteCommand cmd, int id)
        {
            cmd.CommandText = $"DELETE FROM {TableName} WHERE id={id};";
            cmd.ExecuteNonQuery();
            return true;
        }

    }

}
