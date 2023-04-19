using Microsoft.Data.Sqlite;
using System.Data;
using System.Text;

namespace SCCPP1.Database
{
    public class DbCommandHelper
    {

        //protected DatabaseCommand _dbCmd;
        protected StringBuilder _params;



        public DbCommandHelper()//DatabaseCommand dbCmd)
        {

            //_dbCmd = dbCmd;
            _params = new StringBuilder();
        }// TODO:


        //returns a list of ids
 /*       public List<int> InsertOrIgnore(string tableName, List<object> values)
        {
            //clear
            _params.Clear();

            _params.Append($"INSERT OR IGNORE INTO {tableName} VALUES (");
            for (int i = 1; i < values.Count; i++)
                _params.Append($"(@val{i}),");

            //remove comma
            _params.Remove(_params.Length - 1, 1);
            _params.Append(");");

            //set query string
            _dbCmd.CommandType = CommandType.Text;
            _dbCmd.CommandText = _params.ToString();

            for (int i = 0; i < values.Count; i++)
                _dbCmd.Parameters.AddWithValue($"@val{i}", values[i]);

            List<int> ids = new List<int>();

            using (SqliteDataReader r = _dbCmd.ExecuteReader())
            {
                while (r.Read())
                    ids.Add(GetInt32(r, 0));

            }


            return ids;
        }*/

        public DatabaseResponse Insert(string tableName, List<object> values)
        {
            return null;
        }

        public DatabaseResponse Update(string tableName, List<object> values)
        {
            return null;
        }

        public DatabaseResponse Select(string tableName, List<object> values)
        {
            return null;
        }

        public DatabaseResponse Delete(string tableName, List<object> values)
        {
            return null;
        }


        private int GetInt32(SqliteDataReader r, int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return -1;
            return r.GetInt32(ordinal);
        }
    }
}
