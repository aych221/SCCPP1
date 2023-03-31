using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SCCPP1.Database.Entity
{
    public class DbColumn : Field
    {

        public readonly DbTable Table;

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

        public readonly string Name;

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


    public class DbField<T> : DbColumn//  where T : class
    {

        public override object Value { get; set; }


        /*public DbField(DbTable table, string name, int ordinal, bool isPrimaryKey, bool isRequired, bool isUnique, DbColumn foreignKey, T value) :
            base(table, name, ordinal, isPrimaryKey, isRequired, isUnique, foreignKey)
        {
            Value = value;
        }*/

        public DbField(DbColumn column, T value) :
            base(column)
        {
            ValueType = typeof(T);
            Value = value;
        }

    }
}
