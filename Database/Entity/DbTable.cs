using Microsoft.IdentityModel.Tokens;
using SCCPP1.Database.Policies;
using SCCPP1.Database.Sqlite;
using System;
using System.Collections.ObjectModel;

namespace SCCPP1.Database.Entity
{
    public class DbTable
    {

        public readonly ForeignKeyDeletePolicy DefaultDeletePolicyFK;

        private string _name;
        public virtual string Name
        { 
            get { return _name; }
            protected set
            {
                _name = value;
                _quotedName = $"[{_name}]";
            }
        }

        private string _quotedName;
        public virtual string QuotedName
        {
            get { return _quotedName; }
        }


        public readonly string Alias;

        public readonly DbColumn PrimaryKey;

        public DbColumn[]? Columns { get; protected set; }

        public DbColumn[]? ForeignKeys { get; protected set; }

        public bool HasForeignKeys { get; protected set; }


        /// <summary>
        /// Determines if the table is a value storage table. A value storage table is a table that only has two columns, the primary key and the value column.
        /// </summary>
        public bool IsValueStorageOnly { get { return Columns?.Length == 2; } }


        //might want to use stack for this
        /// <summary>
        /// Columns that need to be added to the table.
        /// </summary>
        protected List<DbColumn>? _toAlterColumns;


        private bool _isAltered;

        /// <summary>
        /// Determines if the table needs to run an alter query.
        /// </summary>
        public bool IsAltered
        { 
            get { return _toAlterColumns == null || _toAlterColumns.Count == 0; }
        }


        public QueryStatements Statements { get; protected set; }



        public DbTable(string name, string alias, params Field[] fields)
        {
            Name = name;
            Alias = alias;

            //create primary key field for table
            PrimaryKey = new DbColumn(this, 0, "id", typeof(int), true, true);

            AddCols(fields);

            //don't want to set alter flag to true, so empty after inital AddCols().
            _toAlterColumns = null;
        }





        private int _ordinalCount;

        public DbTable AddCols(params Field[] fields)
        {

            //create new array to accomodate new columns
            DbColumn[] columns;
            if (Columns == null)
                columns = new DbColumn[fields.Length + 1];
            else
                columns = new DbColumn[Columns.Length + fields.Length];


            columns[0] = PrimaryKey;


            //create list for foreign keys
            List<DbColumn> foreignKeys = new List<DbColumn>();

            int i = 1;
            DbColumn col;

            if (Columns != null)
            {
                //add original columns
                for (; i < Columns.Length; i++)
                {
                    col = columns[i] = Columns[i];

                    if (col.IsForeignKey)
                        foreignKeys.Add(col);

                }
            }

            _toAlterColumns = new List<DbColumn>(fields.Length);
            //add new columns
            for (int j = 0; j < fields.Length; j++)
            {
                col = columns[i + j] = new DbColumn(this, _ordinalCount++, fields[j]);
                _toAlterColumns.Add(col);

                if (col.IsForeignKey)
                    foreignKeys.Add(col);

            }

            if (HasForeignKeys = foreignKeys.Count > 0)
                ForeignKeys = foreignKeys.ToArray();

            Columns = columns;

            return this;
        }


        /// <summary>
        /// Copies the columns for this table, starting after the primary key.
        /// </summary>
        /// <param name="columns">an array of table columns without the primary key</param>
        public void CopyColumnsNoPK(ref DbColumn[] columns)
        {
            Array.Copy(Columns, 1, columns, 0, columns.Length);
        }

        /// <summary>
        /// Copies the columns for this table, starting after the primary key.
        /// </summary>
        public DbColumn[] CopyColumnsNoPK()
        {
            DbColumn[] columns = new DbColumn[Columns.Length - 1];
            Array.Copy(Columns, 1, columns, 0, columns.Length);
            return columns;
        }


        /// <summary>
        /// Copies the columns for this table.
        /// </summary>
        /// <param name="columns">an array of table columns</param>
        public void CopyColumns(ref DbColumn[] columns)
        {
            Array.Copy(Columns, columns, Columns.Length);
        }

        /// <summary>
        /// Copies the columns for this table.
        /// </summary>
        public DbColumn[] CopyColumns()
        {
            DbColumn[] columns = new DbColumn[Columns.Length];
            Array.Copy(Columns, columns, Columns.Length);
            return columns;
        }



        public void GenerateQueryStatements()
        {

        }


    }

    public struct QueryStatements
    {
        private string InsertAll { get; set; }
        private string InsertRequiredOnly { get; set; }
        private string InsertOrIgnoreAll { get; set; }
        private string InsertOrIgnoreRequiredOnly { get; set; }
        private string SelectAll { get; set; }
        private string SelectRequiredOnly { get; set; }
        private string UpdateAll { get; set; }
        private string UpdateRequiredOnly { get; set; }
        private string DeleteAll { get; set; }
        private string DeleteFrom { get; set; }


      //  public QueryStatements(DbTable t)
       // {
        //    DbQueryBuilder qb = new DbQueryBuilder();
        //}

    }

}
