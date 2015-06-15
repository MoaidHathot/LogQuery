using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LogQuery.DataAccess.Database
{
    public class SqlServerQueryGenerator : IQueryGenerator
    {
         public string[] GenerateCreateQuery(DataSet set, bool createDatabaseIfNotExists = true, bool createTablesIfNotExists = false, bool useGo = false)
        {
            var list = new List<string>();

            if (createDatabaseIfNotExists)
            {
                list.Add(string.Format(SqlServerQueryConstants.CreateDatabaseIfNotExistsFormat, set.Namespace) + (useGo? Environment.NewLine + "GO" : ""));

                foreach (DataTable table in set.Tables)
                {
                    list.AddRange(GenerateCreateQuery(set.Namespace, table, createTablesIfNotExists, useGo));
                }
            }

            return list.ToArray();
        }

        public string[] GenerateCreateQuery(string database, DataTable table, bool createIfNotExists = false, bool useGo = false)
        {
            var list = new List<string>();

            list.Add(string.Format(SqlServerQueryConstants.UseDatabaseFormat, database));

            var builder = new StringBuilder();

            builder.AppendLine(string.Format("CREATE TABLE [{0}].[{1}](", table.Namespace, table.TableName));
            
            for(var index = 0; index < table.Columns.Count; ++index)
            {
                var column = table.Columns[index];

                builder.Append(string.Format(SqlServerQueryConstants.CreateColumnFormat, column.ColumnName, ToSqlDataType(column.DataType)));

                if (column.AutoIncrement && column.Unique)
                {
                    builder.AppendLine(string.Format(" IDENTITY(1,1)" ));
                }

                if (column.AllowDBNull)
                {
                    builder.Append("NULL");
                }
                else
                {
                    builder.Append("NOT NULL");
                }

                if (index != table.Columns.Count - 1)
                {
                    builder.AppendLine(",");
                }
            }

            builder.AppendLine(") ");

            if (useGo)
            {
                builder.AppendLine("Go");
            }
            

            var query = builder.ToString();

            if (createIfNotExists)
            {
                query = string.Format(SqlServerQueryConstants.CreateTableIfNotExistsFormat, table.Namespace, table.TableName, query) + (useGo ? Environment.NewLine + "GO" : "");
            }

            list.Add(string.Format(SqlServerQueryConstants.CreateSchemaIfNotExistsFormat, table.Namespace) + (useGo ? Environment.NewLine + "GO" : ""));
            list.Add(query);


            return list.ToArray();
        }

        protected string ToSqlDataType(Type type)
        {
            if(type.Equals(typeof(int)) || type.Equals(typeof(int?)))
            {
                return string.Format("[{0}]", SqlDbType.Int.ToString());
            }
            else if (type.Equals(typeof(string)))
            {
                return string.Format("[{0}] (max)", SqlDbType.NVarChar.ToString());
            }
            else if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?)))
            {
                return string.Format("[{0}]", SqlDbType.DateTime.ToString());
            }
            else if (type.Equals(typeof(long)) || type.Equals(typeof(long?)))
            {
                return string.Format("[{0}]", SqlDbType.BigInt.ToString());
            }
            else if (type.Equals(typeof(float)) || type.Equals(typeof(float?)))
            {
                return string.Format("[{0}]", SqlDbType.Real.ToString());
            }
            else if (type.Equals(typeof(double)) || type.Equals(typeof(double?)))
            {
                return string.Format("[{0}]", SqlDbType.Float.ToString());
            }
            else if (type.Equals(typeof(byte)) || type.Equals(typeof(byte?)))
            {
                return string.Format("[{0}]", SqlDbType.TinyInt.ToString());
            }
            else if (type.Equals(typeof(bool)) || type.Equals(typeof(bool?)))
            {
                return string.Format("[{0}]", SqlDbType.Bit.ToString());
            }
            else if (type.Equals(typeof(short)) || type.Equals(typeof(short?)))
            {
                return string.Format("[{0}]", SqlDbType.SmallInt.ToString());
            }
            else if (type.Equals(typeof(TimeSpan)) || type.Equals(typeof(TimeSpan?)))
            {
                return string.Format("[{0}]", SqlDbType.Time.ToString());
            }

            throw new ArgumentException(string.Format("{0} is Not a valid SqlDbType", type.FullName));
        }
    }

    class SqlServerQueryConstants
    {
        public const string UseDatabaseFormat = @"use {0}";

        public const string CreateDatabaseIfNotExistsFormat = @"IF db_id('{0}') IS NULL
        create database {0}";

        public const string CreateSchemaIfNotExistsFormat = @"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
        BEGIN
        EXEC('CREATE SCHEMA {0}')
        END";

        public const string CreateTableIfNotExistsFormat = @"IF NOT (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'))
        BEGIN
        {2}
        END";

        public const string CreateColumnFormat = @"[{0}] {1} ";
    }
}
