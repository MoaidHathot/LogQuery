using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LogQuery.DataAccess.Database
{
    public interface IQueryGenerator
    {
        string[] GenerateCreateQuery(DataSet set, bool createDatabaseIfNotExists, bool createTablesIfNotExists, bool useGo);
        string[] GenerateCreateQuery(string database, DataTable table, bool createIfNotExists, bool useGo);
    }
}
