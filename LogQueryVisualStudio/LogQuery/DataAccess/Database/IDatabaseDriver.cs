using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LogQuery.DataAccess.Database
{
    public interface IDatabaseDriver
    {
        void Save(DataSet set);
        void CretaeSchema(string outputFileName, DataSet set);
    }
}
