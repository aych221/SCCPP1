using static SCCPP1.DatabaseConnector;
using System.Text;
using SCCPP1.Database.Entity;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using DbColumn = SCCPP1.Database.Entity.DbColumn;
using SCCPP1.User;
using SCCPP1.Database.Policies;

namespace SCCPP1.Database
{

    public class DbQueryMaker
    {

    }


    public static class QueryGenerator
    {

        #region Select statements

        /// <summary>
        /// Generates a SELECT statement for all columns in a table.
        /// </summary>
        /// <param name="table">The table to select from.</param>
        /// <returns>A string in the format of "SELECT * FROM [table_name];".</returns>
        public static string SelectAll(DbTable table)
        {
            return $"SELECT * FROM {table.QuotedName};";
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table based on a specified WHERE key. 
        /// </summary>
        /// <param name="whereKey">The DbColumn used as the WHERE key in the SELECT statement.</param>
        /// <returns>A string containing the generated SELECT statement, in the format of "SELECT ... FROM [table] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public static string SelectAll(DbColumn whereKey)
        {
            return Select(whereKey, true, false);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table that are marked as required.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;"</returns>
        public static string SelectRequired(DbColumn whereKey)
        {
            return Select(whereKey, true, true);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table, with the option to include foreign key columns.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public static string Select(DbColumn whereKey, bool joinForeignKeys)
        {
            return Select(whereKey, joinForeignKeys, false);
        }

        /// <summary>
        /// Generates a SELECT statement for a table, with the option to include foreign key columns and to join only required foreign keys.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <param name="joinOnlyRequired">True to join only required foreign keys, false to join all foreign keys.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public static string Select(DbColumn whereKey, bool joinForeignKeys, bool joinOnlyRequired)
        {

            DbTable t = whereKey.Table;

            StringBuilder sb = new StringBuilder($"SELECT {JoinColumns(t.Columns)}");

            if (joinForeignKeys && t.HasForeignKeys)
                sb.Append($",{JoinColumns(t.ForeignKeys, true)}").AppendLine();

            sb.AppendLine($"FROM {t.QuotedName}");

            //Add join clauses
            if (joinForeignKeys)
                foreach (DbColumn c in t.Columns)
                    if (c.IsForeignKey && (!joinOnlyRequired || c.IsRequired))
                         sb.AppendLine(JoinOnClause(c));

            //finally append where clause
            return sb.Append(WhereEqualsClause(whereKey)).Append(';').ToString();
        }
        #endregion


        private static string JoinOnClause(DbColumn onColumn)
        {
            return $"{(onColumn.IsRequired ? "" : "LEFT ")}JOIN {onColumn.ForeignKey.Table.QuotedName} ON {onColumn.QualifiedName} = {onColumn.ForeignKey.Table.PrimaryKey.QualifiedName}";
        }


        private static string WhereEqualsClause(DbColumn onColumn)
        {
            return $"WHERE {onColumn.QualifiedName} = @{onColumn.Name}";
        }


        private static string WhereEqualsClause(DbTable table)
        {
            return $"WHERE {table.PrimaryKey.QualifiedName} = @{table.PrimaryKey.Name}";
        }

        /*
         *      Equals (=)
                Not equals (<>)
                Less than (<)
                Greater than (>)
                Less than or equal to (<=)
                Greater than or equal to (>=)
                IN (value set)
                LIKE (pattern matching)
                BETWEEN (range)
                IS NULL (null value)
         * 
         */
        private static string WhereInClause(DbColumn onColumn, int count)
        {
            StringBuilder sb = new StringBuilder($"WHERE {onColumn.QualifiedName} IN (");
            for (int i = 0; i < count; i++)
                sb.Append($"@{onColumn.Name}{i},");
            return sb.Remove(sb.Length - 1, 1).Append(')').ToString();
        }



        private static string InsertHeader(DbTable table, bool addOrIgnore, bool requiredOnlyColumns)
        {
            return $"INSERT{(addOrIgnore ? " OR IGNORE" : "")} INTO {table.QuotedName} ({(requiredOnlyColumns?JoinRequiredColumns(table.Columns) : JoinColumns(table.Columns))}) VALUES ";
        }



        private static string InsertHeader(DbTable table, bool requiredOnlyColumns)
        {
            return InsertHeader(table, false, requiredOnlyColumns);
        }

        private static string InsertOrIgnoreHeader(DbTable table, bool requiredOnlyColumns)
        {
            return InsertHeader(table, true, requiredOnlyColumns);
        }

        private static string Insert(bool addOrIgnore, DbTable table, params DbRecord[] records)
        {

            StringBuilder sb = new StringBuilder("INSERT");

            if (addOrIgnore)
                sb.Append(" OR IGNORE");

            sb.Append($" INTO {table.QuotedName} ({JoinColumns(records[0].Columns)}) VALUES ");

            StringBuilder record = new StringBuilder();

            //go through each tuple
            for (int i = 0; i < records.Length; i ++)
            {
                //reset tuple string
                record.Clear();
                record.Append('(');

                //go through each column
                foreach (DbColumn col in records[i].Columns)
                {
                    record.Append($"@{col.Name}{i},");
                }

                //remove last comma
                record.Remove(record.Length - 1, 1);

                //apprend to query
                sb.Append(record.Append("),"));
            }


            return sb.Remove(sb.Length - 1, 1).Append(";").ToString();
        }



        public static string Insert(DbTable table, params DbRecord[] records)
        {
            return Insert(false, table, records);
        }

        public static string InsertOrIgnore(DbTable table, params DbRecord[] records)
        {
            return Insert(true, table, records);
        }


        public static DatabaseResponse InsertOrIgnore(SqliteCommand cmd, DbTable table, params DbRecord[] records)
        {
            //pre-command process
            CommandType commandType = cmd.CommandType;
            cmd.CommandType = CommandType.Text;
            string commandText = cmd.CommandText;
            cmd.Parameters.Clear();


            //generate sql string
            string sql = InsertOrIgnore(table, records);
            cmd.CommandText = sql;

            Console.WriteLine(sql);


            DbColumn col;

            //replace all tuple params with values
            for (int i = 0; i < records.Length; i ++)
            {
                for (int j = 0; j < records[i].Columns.Length; j ++)
                {
                    col = records[i].Columns[j];
                    cmd.Parameters.AddWithValue($"{col.Name}{i}", col.Value);
                }
            }

            Console.WriteLine("Rows affected: " + cmd.ExecuteNonQuery());
            

            //post-command process
            cmd.CommandType = commandType;
            cmd.CommandText = commandText;

            return null;
        }

        /// <summary>
        /// Generates an UPDATE statement for a table, setting only the required columns to their corresponding parameter values.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public static string UpdateRequiredOnly(DbTable table)
        {
            return Update(table, false);
        }

        /// <summary>
        /// Generates an UPDATE statement for a table, setting all columns to their corresponding parameter values.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public static string UpdateAll(DbTable table)
        {
            return Update(table, true);
        }


        /// <summary>
        /// Generates an update statement for the specified table, updating only required values or all values based on the <paramref name="updateNonRequiredValues"/> flag.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <param name="updateNonRequiredValues">If true, update non-required columns as well.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public static string Update(DbTable table, bool updateNonRequiredValues)
        {
            StringBuilder sb = new StringBuilder($"UPDATE {table.QuotedName} SET ");

            //start from one since we won't update the id.
            foreach (DbColumn c in table.Columns.Skip(1))
                if (c.IsRequired || updateNonRequiredValues)
                    sb.Append($" {c.QuotedName}=@{c.Name},");

            sb.Remove(sb.Length - 1, 1);

            return sb.Append($" {WhereEqualsClause(table.PrimaryKey)};").ToString();
        }


        public static string Delete(DbTable table)
        {
            return $"DELETE FROM {table.QuotedName} {WhereEqualsClause(table.PrimaryKey)};";
        }



        private static string JoinColumns(DbColumn[] columns, bool useQualifiedName = false)
        {
            //use string.Join since these are likely "small" arrays
            if (useQualifiedName)
                return string.Join(",", columns.Select(c => c.QualifiedName));
            else
                return string.Join(",", columns.Select(c => c.QuotedName));
        }



        private static string JoinRequiredColumns(DbColumn[] columns, bool useQualifiedName = false)
        {
            //use string.Join since these are likely "small" arrays
            if (useQualifiedName)
                return string.Join(",", columns.Where(c => c.IsRequired).Select(c => (c.QualifiedName)));
            else
                return string.Join(",", columns.Where(c => c.IsRequired).Select(c => c.QuotedName));
        }

        private static string JoinColumnsAsParams(bool useIndexer, int index, DbColumn[] columns)
        {
            StringBuilder sb = new StringBuilder($"@{columns[0].Name}{(useIndexer ? index : "" )}");

            for (int i = 1; i < columns.Length; i++)
            {
                sb.Append(",");
                sb.Append($"@{columns[0].Name}{(useIndexer ? index : "")}");
            }

            return sb.ToString();
        }

        private static string JoinParams(string paramName, int amount)
        {
            StringBuilder sb = new StringBuilder($"{paramName}0");

            for (int i = 1; i < amount; i++)
            {
                sb.Append(",");
                sb.Append($"{paramName}{i}");
            }

            return sb.ToString();
        }


        public static string CreateTableSql(DbTable table)
        {
            
            StringBuilder sql = new StringBuilder("CREATE TABLE ");

            sql.Append(table.QuotedName);
            sql.AppendLine(" (");

            sql.Append($"  {table.PrimaryKey.QuotedName} INTEGER PRIMARY KEY AUTOINCREMENT");


            //add field names
            foreach (DbColumn col in table.Columns.Skip(1))
            {

                sql.AppendLine(",");
                sql.Append($"  {col.QuotedName} {GetSqliteType(col.ValueType)}");

                if (col.IsUnique)
                    sql.Append(" UNIQUE");

                if (col.IsRequired)
                    sql.Append(" NOT NULL");
            }


            DbTable fkTable;

            if (table.HasForeignKeys)
            {
                //add foreign key statements
                foreach (DbColumn col in table.ForeignKeys)
                {
                    sql.AppendLine(",");

                    fkTable = col.ForeignKey.Table;
                    sql.Append($"  FOREIGN KEY ({col.QuotedName}) REFERENCES {fkTable.QuotedName}({fkTable.PrimaryKey.QuotedName}) ON DELETE CASCADE");
                }
            }
            return sql.AppendLine().Append(");").ToString();
        }

        //ALTER TABLE colleagues ADD COLUMN main_profile_id INTEGER REFERENCES profiles(id);


        public static string AlterTableAddForeignKeys(DbTable table, params Field[] fields)
        {
            StringBuilder sql = new StringBuilder();

            DbTable fkTable;
            foreach (Field field in fields)
            {
                fkTable = field.ForeignKey.Table;
                sql.AppendLine($"ALTER TABLE {table.QuotedName} ADD COLUMN {field.QuotedName} {GetSqliteType(field.ValueType)} REFERENCES {fkTable.QuotedName}({fkTable.PrimaryKey.QuotedName})");
            }

            return sql.ToString();
        }

        public static string DropTables(List<DbTable> tables)
        {

            StringBuilder sql = new StringBuilder("PRAGMA foreign_keys = 0; BEGIN TRANSACTION;");

            foreach (DbTable table in tables)
            {
                sql.AppendLine($"DROP TABLE IF EXISTS {table.QuotedName};");
            }

            return sql.Append("COMMIT; PRAGMA foreign_keys = 1;").ToString();

        }

        public static string DropTables(Dictionary<string, DbTable> tables)
        {

            StringBuilder sql = new StringBuilder("PRAGMA foreign_keys = 0; BEGIN TRANSACTION;");

            foreach (DbTable table in tables.Values)
            {
                sql.AppendLine($"DROP TABLE IF EXISTS {table.QuotedName};");
            }

            return sql.Append("COMMIT; PRAGMA foreign_keys = 1;").ToString();

        }




        private static string GetOnDeleteAction(ForeignKeyDeletePolicy action)
        {
            switch (action)
            {
                case ForeignKeyDeletePolicy.Cascade:
                    return "ON DELETE CASCADE";
                case ForeignKeyDeletePolicy.Restrict:
                    return "ON DELETE RESTRICT";
                case ForeignKeyDeletePolicy.SetNull:
                    return "ON DELETE SET NULL";
                case ForeignKeyDeletePolicy.SetDefault:
                    return "ON DELETE SET DEFAULT";
                case ForeignKeyDeletePolicy.NoAction:
                    return "ON DELETE NO ACTION";
                case ForeignKeyDeletePolicy.None:
                default:
                    return "";
            }
        }

        private static string GetSqliteType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return "INTEGER";

                case TypeCode.DateTime:
                    return "DATE";

                case TypeCode.Char:
                case TypeCode.String:
                default:
                    return "TEXT";
                    
            }
        }


    }
}
