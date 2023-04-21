using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Policies;
using System.Text;

namespace SCCPP1.Database.Sqlite
{
    public class DbQueryBuilder
    {

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


        protected StringBuilder m_query;


        protected DbTable m_currentTable;
        protected DbTable _currentTable
        {
            get { return m_currentTable; }
            set
            {
                m_currentTable = value;
            }
        }


        protected DbColumn m_currentColumn;
        protected DbColumn _currentColumn
        {
            get { return m_currentColumn; }
            set
            {
                m_currentColumn = value;
                _currentTable = value.Table;
            }
        }



        public int Length
        {
            get { return m_query.Length; }
            protected set
            {
                m_query.Length = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DbQueryBuilder"/> class with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size to be used for the <see cref="StringBuilder"/> backing the <see cref="DbQueryBuilder"/> object. Default is 128 bytes.</param>
        public DbQueryBuilder(int bufferSize = 128)
        {
            
            m_query = new StringBuilder(bufferSize);
        }

        public DbQueryBuilder(string query)
        {
            m_query = new StringBuilder(query);
        }


        #region core methods


        public DbQueryBuilder Append(char c)
        {
            m_query.Append(c);
            return this;
        }


        public DbQueryBuilder Append(string query)
        {
            m_query.Append(query);
            return this;
        }


        public DbQueryBuilder AppendLine(string query)
        {
            m_query.AppendLine(query);
            return this;
        }


        public DbQueryBuilder AppendLine()
        {
            m_query.AppendLine();
            return this;
        }



        public DbQueryBuilder Remove(int startIndex, int length)
        {
            m_query.Remove(startIndex, length);
            return this;
        }


        public DbQueryBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            m_query.Replace(oldChar, newChar, startIndex, count);
            return this;
        }



        public DbQueryBuilder Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            m_query.Replace(oldValue, newValue, startIndex, count);
            return this;
        }


        public DbQueryBuilder Replace(char oldChar, char newChar)
        {
            return Replace(oldChar, newChar, 0, m_query.Length);
        }



        public DbQueryBuilder Clear()
        {
            m_query.Clear();
            return this;
        }


        #endregion



        #region Select statements

        /// <summary>
        /// Generates a SELECT statement for all columns in a table.
        /// </summary>
        /// <param name="table">The table to select from.</param>
        /// <returns>A string in the format of "SELECT * FROM [table_name];".</returns>
        public DbQueryBuilder SelectAll(DbTable table)
        {

            return Append("SELECT * FROM ").Append(table.QuotedName).Append(';');
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table based on a specified WHERE key. 
        /// </summary>
        /// <param name="whereKey">The DbColumn used as the WHERE key in the SELECT statement.</param>
        /// <returns>A string containing the generated SELECT statement, in the format of "SELECT ... FROM [table] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public DbQueryBuilder SelectAll(DbColumn whereKey)
        {
            return Select(whereKey, true, false);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table that are marked as required.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;"</returns>
        public DbQueryBuilder SelectRequired(DbColumn whereKey)
        {
            return Select(whereKey, true, true);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table, with the option to include foreign key columns.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public DbQueryBuilder Select(DbColumn whereKey, bool joinForeignKeys)
        {
            return Select(whereKey, joinForeignKeys, false);
        }

        /// <summary>
        /// Generates a SELECT statement for a table, with the option to include foreign key columns and to join only required foreign keys.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <param name="requiredOnly">True to join only required foreign keys, false to join all foreign keys.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public DbQueryBuilder Select(DbColumn whereKey, bool joinForeignKeys, bool requiredOnly)
        {

            _currentColumn = whereKey;

            Append("SELECT ").Append(JoinColumns(requiredOnly));

            if (joinForeignKeys && _currentTable.HasForeignKeys)
                Append(',').Append(JoinFKColumnsNoPK(requiredOnly, true)).AppendLine();

            Append("FROM ").Append(_currentTable.QuotedName).AppendLine();

            //Add join clauses
            if (joinForeignKeys)
                foreach (DbColumn c in _currentTable.Columns)
                    if (c.IsForeignKey && (!requiredOnly || c.IsRequired))
                        AppendLine(JoinOnClause(c));

            //finally append where clause
            return Append(WhereEqualsClause(whereKey)).Append(';');
        }




        /// <summary>
        /// Generates a SELECT statement for a table, with the option to include foreign key columns and to join only required foreign keys.
        /// </summary>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <param name="requiredOnly">True to join only required foreign keys, false to join all foreign keys.</param>
        /// <returns>a QueryString object in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public DbQueryBuilder Select(bool joinForeignKeys, bool requiredOnly)
        {
            return Select(_currentColumn, joinForeignKeys, requiredOnly);
        }



        #endregion



        #region Insert statements



        /*        private string InsertHeader(DbTable table, bool requiredOnlyColumns)
                {
                    return InsertHeader(table, false, requiredOnlyColumns);
                }

                private string InsertOrIgnoreHeader(DbTable table, bool requiredOnlyColumns)
                {
                    return InsertHeader(table, true, requiredOnlyColumns);
                }



                private string InsertHeader(DbTable table, bool addOrIgnore, bool requiredOnlyColumns)
                {
                    return $"INSERT{(addOrIgnore ? " OR IGNORE" : "")} INTO {table.QuotedName} ({(requiredOnlyColumns ? JoinRequiredColumns(table.Columns) : JoinColumns(table.Columns))}) VALUES ";
                }*/


        public DbQueryBuilder Insert(DbRecordData records)
        {
            return Insert(false, records);
        }

        public DbQueryBuilder InsertOrIgnore(DbRecordData records)
        {
            return Insert(true, records);
        }


        public DbQueryBuilder Insert(DbRecordCollection records)
        {
            return Insert(false, records);
        }

        public DbQueryBuilder InsertOrIgnore(DbRecordCollection records)
        {
            return Insert(true, records);
        }


        private DbQueryBuilder Insert(bool addOrIgnore, DbRecordCollection collection)
        {
            _currentTable = collection.Table;
            Append("INSERT");

            if (addOrIgnore)
                Append(" OR IGNORE");

            Append(" INTO ")
                .Append(_currentTable.QuotedName)
                .Append(' ')
                .Append(JoinColumnsNoPK(_currentTable.Columns, false))
                .Append(" VALUES ");


            StringBuilder record = new StringBuilder();
            //go through each tuple (skip id)
            for (int i = 1; i < collection.Count; i++)
            {
                //reset tuple string
                record.Clear();
                record.Append('(');
                
                //go through each column
                for (int j = 0; j < collection[i].ColumnsAndValues.Length; j ++)
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


        private DbQueryBuilder Insert(bool addOrIgnore, DbRecordData record)
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
        public DbQueryBuilder UpdateRequiredOnly(DbTable table)
        {
            return Update(table, false);
        }

        /// <summary>
        /// Generates an UPDATE statement for a table, setting all columns to their corresponding parameter values.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public DbQueryBuilder UpdateAll(DbTable table)
        {
            return Update(table, true);
        }


        /// <summary>
        /// Generates an update statement for the specified table, updating only required values or all values based on the <paramref name="updateNonRequiredValues"/> flag.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <param name="updateNonRequiredValues">If true, update non-required columns as well.</param>
        /// <returns>A string in the format of "UPDATE [table] SET [col1]=@col1, [col2]=@col2, ... WHERE [primaryKey]=@primaryKey;".</returns>
        public DbQueryBuilder Update(DbTable table, bool updateNonRequiredValues)
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
        public DbQueryBuilder DeleteIDs(DbTable table, int[] ids)
        {
            return Append("DELETE FROM ").Append(table.QuotedName).Append(' ').Append(WhereIDIsInClause(table.PrimaryKey, ids)).Append(';');
        }

        public DbQueryBuilder Delete(DbTable table)
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
        public DbQueryBuilder Return(DbColumn column)
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
            return m_query.ToString();
        }

    }

}
