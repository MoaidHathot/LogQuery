using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace LogQuery.DataAccess.Database
{
    public class ManualDatabaseDriver : IDatabaseDriver
    {
        private SqlServerQueryGenerator _queryGenerator = new SqlServerQueryGenerator();
        private string _connectionString;

        public ManualDatabaseDriver(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Save(DataSet set)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var tables = new DataTable[set.Tables.Count];
                set.Tables.CopyTo(tables, 0);

                connection.Open();

                foreach (DataTable table in set.Tables)
                {
                    //var selectQuery = tables.Select(t => string.Format("select * from {0}.{1}.{2} where 0 = 1", set.Namespace, t.Namespace, t.TableName)).Aggregate((a, b) => a + "; " + b);
                    var selectQuery = string.Format("select * from {0}.{1}.{2}", set.Namespace, table.Namespace, table.TableName);

                    using (var adapter = new SqlDataAdapter(selectQuery, connection))
                    {
                        var builder = new SqlCommandBuilder(adapter);

                        adapter.InsertCommand = builder.GetInsertCommand();
                        adapter.Update(table);
                    }
                }

                connection.Close();
            }
        }

        public void CretaeSchema(DataSet set)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    connection.Open();

                    foreach (var query in _queryGenerator.GenerateCreateQuery(set).Select(s => s.Replace("\r\n", " ")))
                    {
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
        }
    }
}
