using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;

namespace SCCPP1.Database
{
    public class DatabaseCommand : SqliteCommand
    {

        public DatabaseCommand(DatabaseConnection connection) : base("", connection)
        {

        }

    }
}
