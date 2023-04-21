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
/*

        public static class Placeholders
        {

            public static string TableName => DbConstants.QueryStringPlaceholders.TableName;
            public static string Columns => DbConstants.QueryStringPlaceholders.ColumnNames;

            public static string Values => DbConstants.QueryStringPlaceholders.Values;

            public static string ColumnsEqualsValues => DbConstants.QueryStringPlaceholders.ColumnsEqualsValues;

            public static string Returning => DbConstants.QueryStringPlaceholders.ReturningClause;

            public static string Where => DbConstants.QueryStringPlaceholders.WhereClause;

            public static string OnDelete => DbConstants.QueryStringPlaceholders.DeleteClause;

            public static string Join => DbConstants.QueryStringPlaceholders.JoinClause;

            public static string LeftJoin => DbConstants.QueryStringPlaceholders.LeftJoinClause;

            public static string PrimaryKeyClause => DbConstants.QueryStringPlaceholders.PrimaryKeyClause;

            public static string ForeignKeyClause => DbConstants.QueryStringPlaceholders.ForeignKeyClause;

        }


        public static class QueryTypes
        {
            private static string _insert => $"{Placeholders.TableName} ({Placeholders.Columns}) VALUES ({Placeholders.Values})";
            private static string _select => $"SELECT {Placeholders.Columns} FROM {Placeholders.TableName}";


            public static string Insert => $"INSERT INTO {_insert};";
            public static string InsertReturning => $"INSERT INTO {_insert} {Placeholders.Returning};";

            public static string InsertOrIgnore => $"INSERT OR IGNORE INTO {_insert};";


            public static string Select => $"{_select};";
            public static string SelectWhere => $"{_select} {Placeholders.Where};";
            public static string SelectJoin => $"{_select} {Placeholders.Join};";
            public static string SelectLeftJoin => $"{_select} {Placeholders.LeftJoin};";
            public static string SelectBothJoin => $"{_select} {Placeholders.Join} {Placeholders.LeftJoin};";
            public static string SelectJoinWhere => $"{_select} {Placeholders.Join} {Placeholders.Where};";
            public static string SelectLeftJoinWhere => $"{_select} {Placeholders.LeftJoin} {Placeholders.Where};";
            public static string SelectBothJoinWhere => $"{_select} {Placeholders.Join} {Placeholders.LeftJoin} {Placeholders.Where};";


            public static string Update => $"UPDATE {Placeholders.TableName} SET {Placeholders.ColumnsEqualsValues} {Placeholders.Where};";

            public static string Delete => $"DELETE FROM {Placeholders.TableName} {Placeholders.Where};";

            public static string Create => $"CREATE TABLE IF NOT EXISTS {Placeholders.TableName} ({Placeholders.Columns});";

            public static string Drop => $"DROP TABLE IF EXISTS {Placeholders.TableName};";

            public static string Alter => $"ALTER TABLE {Placeholders.TableName} {Placeholders.ForeignKeyClause} {Placeholders.OnDelete} {Placeholders.PrimaryKeyClause};";
        
        }

        //TODO add change table method and add configuration for query building
        //for example, add options for setting default name type when generating query strings
        // DefaultNameType = NameType.QualifiedName or NameType.QuotedName
        //DefaultFKNameType = NameType.QualifiedName or NameType.QuotedName
        //DefaultPKNameType = NameType.QualifiedName or NameType.QuotedName
        //DefaultColumnNameType = NameType.QualifiedName or NameType.QuotedName
        //DefaultTableNameType = NameType.QualifiedName or NameType.QuotedName
        //UseFKAlias = true or false
        //UsePKAlias = true or false
        //UseColumnAlias = true or false


        protected StringBuilder _queryString;


        public int Length
        {
            get { return _queryString.Length; }
            protected set
            {
                _queryString.Length = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DbQueryBuilder"/> class with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size to be used for the <see cref="StringBuilder"/> backing the <see cref="DbQueryBuilder"/> object. Default is 128 bytes.</param>
        public DbQueryMaker(int bufferSize = 128)
        {

            _queryString = new StringBuilder(bufferSize);
        }

        public DbQueryMaker(string query)
        {
            _queryString = new StringBuilder(query);
        }


        #region core methods


        public DbQueryMaker Append(char c)
        {
            _queryString.Append(c);
            return this;
        }


        public DbQueryMaker Append(string query)
        {
            _queryString.Append(query);
            return this;
        }


        public DbQueryMaker AppendLine(string query)
        {
            _queryString.AppendLine(query);
            return this;
        }


        public DbQueryMaker AppendLine()
        {
            _queryString.AppendLine();
            return this;
        }



        public DbQueryMaker Remove(int startIndex, int length)
        {
            _queryString.Remove(startIndex, length);
            return this;
        }


        public DbQueryMaker Replace(char oldChar, char newChar, int startIndex, int count)
        {
            _queryString.Replace(oldChar, newChar, startIndex, count);
            return this;
        }



        public DbQueryMaker Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            _queryString.Replace(oldValue, newValue, startIndex, count);
            return this;
        }


        public DbQueryMaker Replace(char oldChar, char newChar)
        {
            return Replace(oldChar, newChar, 0, _queryString.Length);
        }



        public DbQueryMaker Clear()
        {
            _queryString.Clear();
            return this;
        }


        #endregion



        #region Select statements
        
        public DbQueryMaker Select(string table)
        {
            return Select(table, "*");
        }

        public DbQueryMaker Select(string table, string columns)
        {
            return SelectWhere(table, columns, null);
        }

        public DbQueryMaker SelectWhere(string table, string columns, string? whereClause)
        {
            return SelectWhere(table, columns, null, null, whereClause);
        }

        public DbQueryMaker SelectWhere(string table, string columns, string? joins, string? leftJoins, string? whereClause)
        {

            Append("SELECT ").Append(columns).Append(" FROM ").Append(table);

            if (!string.IsNullOrEmpty(joins))
                Append(joins);

            if (!string.IsNullOrEmpty(leftJoins))
                Append(leftJoins);

            if (!string.IsNullOrEmpty(whereClause))
                Append(whereClause);

            return Append(';');
        }
        #endregion



        #region Insert statements
        public DbQueryMaker Insert(DbRecordData records)
        {
            return Insert(false, records);
        }

        public DbQueryMaker InsertOrIgnore(DbRecordData records)
        {
            return Insert(true, records);
        }


        public DbQueryMaker Insert(DbRecordCollection records)
        {
            return Insert(false, records);
        }

        public DbQueryMaker InsertOrIgnore(DbRecordCollection records)
        {
            return Insert(true, records);
        }


        private DbQueryMaker Insert(bool addOrIgnore, string table, string columns)
        {
            Append("INSERT");

            if (addOrIgnore)
                Append(" OR IGNORE");
            
            Append(" INTO ")
                .Append(table)
                .Append(' ')
                .Append(columns)
                .Append(" VALUES ");


            StringBuilder record = new StringBuilder();
            //go through each tuple (skip id)
            for (int i = 1; i < collection.Count; i++)
            {
                //reset tuple string
                record.Clear();
                record.Append('(');

                //go through each column
                for (int j = 0; j < collection[i].ColumnsAndValues.Length; j++)
                {
                    collection[i].ColumnsAndValues[j].Key = $"@{collection[i].ColumnsAndValues[j].Key}{i}";
                    record.Append($"{collection[i].ColumnsAndValues[j].Key},");
                }

                //remove last comma
                record.Remove(record.Length - 1, 1);

                //apprend to query
                Append(record.Append("),").ToString());
            }


            return Remove(Length - 1, 1).Append(";");
        }


        private DbQueryMaker Insert(bool addOrIgnore, DbRecordData record)
        {
            _currentTable = record.Table;
            Append("INSERT");

            if (addOrIgnore)
                Append(" OR IGNORE");

            Append(" INTO ")
                .Append(_currentTable.QuotedName)
                .Append(' ')
                .Append(JoinColumnsNoPK(_currentTable.Columns, false))
                .Append(" VALUES (");

            //go through each column (skip id)
            for (int i = 1; i < record.ColumnsAndValues.Length; i++)
            {
                Append($"@{record.ColumnsAndValues[i].Key},");
            }

            //remove last comma
            return Remove(Length - 1, 1).Append(");");
        }
        #endregion



        #region Update statements
        /// <summary>
        /// Generates an UPDATE statement for a table, setting only the required columns to their corresponding parameter values.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public DbQueryMaker UpdateRequiredOnly(DbTable table)
        {
            return Update(table, false);
        }

        /// <summary>
        /// Generates an UPDATE statement for a table, setting all columns to their corresponding parameter values.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public DbQueryMaker UpdateAll(DbTable table)
        {
            return Update(table, true);
        }


        /// <summary>
        /// Generates an update statement for the specified table, updating only required values or all values based on the <paramref name="updateNonRequiredValues"/> flag.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <param name="updateNonRequiredValues">If true, update non-required columns as well.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public DbQueryMaker Update(DbTable table, bool updateNonRequiredValues)
        {
            Append("UPDATE ").Append(table.QuotedName).Append(" SET");

            //start from one since we won't update the id.
            foreach (DbColumn c in table.Columns.Skip(1))
                if (c.IsRequired || updateNonRequiredValues)
                    Append($" {c.QuotedName}=@{c.Name},");

            Remove(Length - 1, 1);

            return Append($" {WhereEqualsClause(table.PrimaryKey)};");
        }
        #endregion


        #region Delete statements
        public DbQueryMaker DeleteIDs(DbTable table, int[] ids)
        {
            return Append("DELETE FROM ").Append(table.QuotedName).Append(' ').Append(WhereIDIsInClause(table.PrimaryKey, ids)).Append(';');
        }

        public DbQueryMaker Delete(DbTable table)
        {
            return Append("DELETE FROM ").Append(table.QuotedName).Append(' ').Append(WhereEqualsClause(table.PrimaryKey)).Append(';');
        }
        #endregion


        #region other statements
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <returns>"RETURNING [columnName];</returns>
        public DbQueryMaker Return(DbColumn column)
        {
            return Append(ReturingClause(column)).Append(';');
        }


        #endregion



        #region clause methods

        private static string ReturingClause(DbColumn column)
        {
            return $"RETURNING {column.QuotedName}";
        }

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

        private string WhereIDIsInClause(DbColumn onColumn, int[] ids)
        {
            Append($"WHERE {onColumn.QualifiedName} IN (");
            for (int i = 0; i < ids.Length; i++)
                Append($"{i},");
            return Remove(Length - 1, 1).Append(')').ToString();
        }
        private string WhereInClause(DbColumn onColumn, int count)
        {
            Append($"WHERE {onColumn.QualifiedName} IN (");
            for (int i = 0; i < count; i++)
                Append($"@{onColumn.Name}{i},");
            return Remove(Length - 1, 1).Append(')').ToString();
        }
        #endregion



        #region helper methods

        private string JoinAllColumns(bool useQualifiedName = false)
        {
            return JoinColumns(false, useQualifiedName);
        }

        private string JoinAllColumnsNoPK(bool useQualifiedName = false)
        {
            return JoinColumnsNoPK(false, useQualifiedName);
        }



        private string JoinRequiredColumns(bool useQualifiedName = false)
        {
            return JoinColumns(true, useQualifiedName);
        }



        private string JoinRequiredColumnsNoPK(bool useQualifiedName = false)
        {
            return JoinColumnsNoPK(true, useQualifiedName);
        }



        private string JoinAllFKColumns(bool useQualifiedName = false)
        {
            return JoinFKColumns(false, useQualifiedName);
        }



        private string JoinRequiredFKColumns(bool useQualifiedName = false)
        {
            return JoinFKColumns(true, useQualifiedName);
        }



        private string JoinFKColumnsNoPK(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinFKColumns(_currentTable.ForeignKeys, true, requiredOnly, useQualifiedName);
        }



        private string JoinFKColumns(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinFKColumns(_currentTable.ForeignKeys, false, requiredOnly, useQualifiedName);
        }

        private string JoinFKColumns(DbColumn[] arr, bool skipPrimaryKey, bool requiredOnly, bool useQualifiedName = false)
        {

            var columns = requiredOnly ? arr.Where(c => c.IsRequired) : arr;

            if (skipPrimaryKey)
                columns = columns.Skip(1);

            return string.Join(",", columns.Select(c =>
            {
                //table has only two columns, grab the value column
                if (c.ForeignKey.Table.IsValueStorageOnly)
                    return c.ForeignKey.Table.Columns[1].QualifiedName;
                else
                    return c.ForeignKey.QualifiedName;

            }));
        }



        private string JoinColumns(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumns(_currentTable.Columns, requiredOnly, useQualifiedName);
        }


        private string JoinColumnsNoPK(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumnsNoPK(_currentTable.Columns, requiredOnly, useQualifiedName);
        }



        private string JoinColumns(DbColumn[] arr, bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumns(arr, false, requiredOnly, useQualifiedName);
        }

        private string JoinColumnsNoPK(DbColumn[] arr, bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumns(arr, true, requiredOnly, useQualifiedName);
        }

        private string JoinColumns(DbColumn[] arr, bool skipPrimaryKey, bool requiredOnly, bool useQualifiedName = false)
        {
            var columns = requiredOnly ? arr.Where(c => c.IsRequired) : arr;

            if (skipPrimaryKey)
                columns = columns.Skip(1);

            //use string.Join since these are likely "small" arrays
            if (useQualifiedName)
                return string.Join(",", columns.Select(c => c.QualifiedName));
            else
                return string.Join(",", columns.Select(c => c.QuotedName));
        }


        private string JoinColumnsAsParams(bool useIndexer, int index)
        {
            Append($"@{_currentTable.Columns[0].Name}{(useIndexer ? index : "")}");

            for (int i = 1; i < _currentTable.Columns.Length; i++)
            {
                Append(",");
                Append($"@{_currentTable.Columns[0].Name}{(useIndexer ? index : "")}");
            }

            return ToString();
        }

        private string JoinParams(string paramName, int amount)
        {
            Append($"{paramName}0");

            for (int i = 1; i < amount; i++)
            {
                Append(",");
                Append($"{paramName}{i}");
            }

            return ToString();
        }

        #endregion




        #region static methods



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
        #endregion




        public override string ToString()
        {
            return _queryString.ToString();
        }*/

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
