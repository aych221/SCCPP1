using static SCCPP1.DatabaseConnector;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database.Entity;
using System.Runtime.CompilerServices;
using System;
using Microsoft.Data.Sqlite;
using System.Data;
using SCCPP1.Models;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Metadata;
using static Azure.Core.HttpHeader;
using System.Drawing;

namespace SCCPP1.Database
{
    public class DbQueryMaker
    {

    }


    public static class QueryGenerator
    {
        
        private static string Insert(bool addOrIgnore, DbTable table, params DbRecord[] records)
        {

            StringBuilder sql = new StringBuilder("INSERT");

            if (addOrIgnore)
                sql.Append(" OR IGNORE");

            sql.Append($" INTO {table.Name} ({JoinColumns(records[0].Columns)}) VALUES ");

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
                sql.Append(record.Append("),"));
            }


            return sql.Remove(sql.Length - 1, 1).Append(";").ToString();
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


        private static string JoinColumns(params DbColumn[] columns)
        {
            StringBuilder sb = new StringBuilder(columns[0].Name);

            for (int i = 1; i < columns.Length; i++)
            {
                sb.Append(",");
                sb.Append(columns[i].Name);
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

            sql.Append(table.Name);
            sql.AppendLine(" (");

            sql.Append($"  {table.PrimaryKey.Name} INTEGER PRIMARY KEY AUTOINCREMENT");


            //add field names
            foreach (DbColumn col in table.Columns.Skip(1))
            {

                sql.AppendLine(",");
                sql.Append($"  {col.Name} {TypeToSqliteQuery(col.ValueType)}");

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
                    sql.Append($"  FOREIGN KEY ({col.Name}) REFERENCES {fkTable.Name}({fkTable.PrimaryKey.Name})");
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
                sql.AppendLine($"ALTER TABLE {table.Name} ADD COLUMN {field.Name} {TypeToSqliteQuery(field.ValueType)} REFERENCES {fkTable.Name}({fkTable.PrimaryKey.Name})");
            }

            return sql.ToString();
        }

        public static string DropTables(List<DbTable> tables)
        {

            StringBuilder sql = new StringBuilder("PRAGMA foreign_keys = 0; BEGIN TRANSACTION;");

            foreach (DbTable table in tables)
            {
                sql.AppendLine($"DROP TABLE IF EXISTS [{table.Name}];");
            }

            return sql.Append("COMMIT; PRAGMA foreign_keys = 1;").ToString();

        }

        private static string TypeToSqliteQuery(Type t)
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
