using SCCPP1.Database.Entity;
using SCCPP1.Database.Policies;
using System.Text;

namespace SCCPP1.Database.Sqlite
{
    public class QueryString
    {

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
        /// Initializes a new instance of the <see cref="QueryString"/> class with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size to be used for the <see cref="StringBuilder"/> backing the <see cref="QueryString"/> object. Default is 128 bytes.</param>
        public QueryString(int bufferSize = 128)
        {
            
            m_query = new StringBuilder(bufferSize);
        }

        public QueryString(string query)
        {
            m_query = new StringBuilder(query);
        }


        #region core methods


        public QueryString Append(char c)
        {
            m_query.Append(c);
            return this;
        }


        public QueryString Append(string query)
        {
            m_query.Append(query);
            return this;
        }


        public QueryString AppendLine(string query)
        {
            m_query.AppendLine(query);
            return this;
        }


        public QueryString AppendLine()
        {
            m_query.AppendLine();
            return this;
        }



        public QueryString Remove(int startIndex, int length)
        {
            m_query.Remove(startIndex, length);
            return this;
        }


        public QueryString Replace(char oldChar, char newChar, int startIndex, int count)
        {
            m_query.Replace(oldChar, newChar, startIndex, count);
            return this;
        }



        public QueryString Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            m_query.Replace(oldValue, newValue, startIndex, count);
            return this;
        }


        public QueryString Replace(char oldChar, char newChar)
        {
            return Replace(oldChar, newChar, 0, m_query.Length);
        }



        public QueryString Clear()
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
        public QueryString SelectAll(DbTable table)
        {

            return Append("SELECT * FROM ").Append(table.QuotedName).Append(';');
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table based on a specified WHERE key. 
        /// </summary>
        /// <param name="whereKey">The DbColumn used as the WHERE key in the SELECT statement.</param>
        /// <returns>A string containing the generated SELECT statement, in the format of "SELECT ... FROM [table] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public QueryString SelectAll(DbColumn whereKey)
        {
            return Select(whereKey, true, false);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table that are marked as required.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;"</returns>
        public QueryString SelectRequired(DbColumn whereKey)
        {
            return Select(whereKey, true, true);
        }

        /// <summary>
        /// Generates a SELECT statement for all columns in a table, with the option to include foreign key columns.
        /// </summary>
        /// <param name="whereKey">The key that is used in the where statement.</param>
        /// <param name="joinForeignKeys">True to include foreign key columns in the SELECT statement, false otherwise.</param>
        /// <returns>a string in the format of "SELECT ... FROM [whereKeyTable] ... JOIN or LEFT JOIN ... WHERE [whereKey] = @whereKey;".</returns>
        public QueryString Select(DbColumn whereKey, bool joinForeignKeys)
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
        public QueryString Select(DbColumn whereKey, bool joinForeignKeys, bool requiredOnly)
        {

            m_currentColumn = whereKey;

            Append("SELECT ").Append(JoinColumns(requiredOnly));

            if (joinForeignKeys && _currentTable.HasForeignKeys)
                Append(',').Append(JoinFKColumns(requiredOnly, true)).AppendLine();

            Append("FROM ").Append(m_currentTable.QuotedName).AppendLine();

            //Add join clauses
            if (joinForeignKeys)
                foreach (DbColumn c in m_currentTable.Columns)
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
        public QueryString Select(bool joinForeignKeys, bool requiredOnly)
        {
            return Select(m_currentColumn, joinForeignKeys, requiredOnly);
        }



        #endregion




        #region clause methods
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



        private string JoinRequiredColumns(bool useQualifiedName = false)
        {
            return JoinColumns(true, useQualifiedName);
        }



        private string JoinAllFKColumns(bool useQualifiedName = false)
        {
            return JoinFKColumns(false, useQualifiedName);
        }



        private string JoinRequiredFKColumns(bool useQualifiedName = false)
        {
            return JoinFKColumns(true, useQualifiedName);
        }



        private string JoinFKColumns(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumns(m_currentTable.ForeignKeys, requiredOnly, useQualifiedName);
        }



        private string JoinColumns(bool requiredOnly, bool useQualifiedName = false)
        {
            return JoinColumns(m_currentTable.Columns, requiredOnly, useQualifiedName);
        }



        private string JoinColumns(DbColumn[] arr, bool requiredOnly, bool useQualifiedName = false)
        {
            var columns = requiredOnly ? arr.Where(c => c.IsRequired) : arr;

            //use string.Join since these are likely "small" arrays
            if (useQualifiedName)
                return string.Join(",", columns.Select(c => c.QualifiedName));
            else
                return string.Join(",", columns.Select(c => c.QuotedName));
        }


        private string JoinColumnsAsParams(bool useIndexer, int index)
        {
            Append($"@{m_currentTable.Columns[0].Name}{(useIndexer ? index : "")}");

            for (int i = 1; i < m_currentTable.Columns.Length; i++)
            {
                Append(",");
                Append($"@{m_currentTable.Columns[0].Name}{(useIndexer ? index : "")}");
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
