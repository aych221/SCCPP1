using System;

namespace SCCPP1.Database.Entity
{
    public class DbTable
    {

        private int _ordinal;

        public readonly string Name;

        public readonly DbColumn PrimaryKey;

        public DbColumn[] Columns { get; protected set; }

        public DbColumn[] ForeignKeys { get; protected set; }

        public bool HasForeignKeys { get; protected set; }



        public DbTable(string name, params Field[] fields)
        {
            Name = name;

            //create primary key field for table
            PrimaryKey = new DbColumn(this, 0, "id", typeof(int), true, true);

            AddCols(fields);

        }


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

            //add new columns
            for (int j = 0; j < fields.Length; j++)
            {
                col = columns[i + j] = new DbColumn(this, _ordinal++, fields[j]);

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
    }
}
