using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database.Policies;

namespace SCCPP1.Database.Entity
{
    public class DbColumn : Field
    {

        public readonly ForeignKeyDeletePolicy DeletePolicyFK;



        private DbTable _table;

        public DbTable Table
        {
            get { return _table; }
            protected set
            {
                _table = value;
                QualifiedName = $"[{Table.Name}].{QuotedName}";
            }
        }


        public override string Name
        {
            protected set
            {
                base.Name = value;

                if (Table != null)
                    QualifiedName = $"[{Table.Name}].{QuotedName}";
            }
        }


        private string _qualifiedName;

        /// <summary>
        /// The fully qualified i.e. [table_name].[column_name]
        /// </summary>
        public string QualifiedName
        {
            get { return _qualifiedName; }
            protected set
            {
                _qualifiedName = value;
            }
        }


        public readonly int Ordinal;

        //public new readonly DbColumn ForeignKey;

        public virtual object Value { get; set; }




        public DbColumn(DbTable table, int ordinal, string name, Type valueType, bool isRequired, bool isUnique, DbColumn foreignKey = null) :
            base(name, valueType, isRequired, isUnique, foreignKey)
        {
            Table = table;
            Ordinal = ordinal;
        }

        public DbColumn(DbTable table, int ordinal, DbColumn column) :
            this(table, ordinal, column.Name, column.ValueType, column.IsRequired, column.IsUnique, column.ForeignKey)
        {

        }

        public DbColumn(DbTable table, int ordinal, Field field) :
            this(table, ordinal, field.Name, field.ValueType, field.IsRequired, field.IsUnique, field.ForeignKey)
        {

        }


        public DbColumn(DbColumn column) :
            this(column.Table, column.Ordinal, column.Name, column.ValueType, column.IsRequired, column.IsUnique, column.ForeignKey)
        {

        }


    }


    public class Field
    {


        private string _name;

        public virtual string Name
        {
            get { return _name; }
            protected set
            {
                _name = value;
                QuotedName = _name;
            }
        }


        private string _quotedName;

        /// <summary>
        /// The quoted identifier i.e. [column_name]
        /// </summary>
        public virtual string QuotedName
        {
            get { return _quotedName; }
            protected set
            {
                _quotedName = $"[{value}]";
            }
        }


        public Type ValueType { get; protected set; }

        public readonly bool IsRequired, IsUnique, IsForeignKey;

        public readonly DbColumn ForeignKey;


        public Field(string name, Type valueType, bool isRequired, bool isUnique, DbColumn foreignKey = null)
        {
            Name = name;
            ValueType = valueType;
            IsRequired = isRequired;
            IsUnique = isUnique;
            IsForeignKey = (ForeignKey = foreignKey) != null;
        }

    }
}
