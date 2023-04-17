using Microsoft.Data.Sqlite;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Sqlite;
using SCCPP1.User;

namespace SCCPP1.Database
{
    public class DbAssistant
    {

        public DbRecordCollection Records;

        public string LastSql { get; private set; }

        public DbQueryString Sql;



        public DbAssistant(DbRecordCollection records)
        {
            Records = records;
            Sql = new DbQueryString();
        }


        public bool SaveToDatabase(Account account)
        {
            Sql.SelectRequired(Records[0].Table.PrimaryKey);
            return true;
        }


        public bool LoadToAccount(Account account)
        {
            return true;
        }



    }
}
