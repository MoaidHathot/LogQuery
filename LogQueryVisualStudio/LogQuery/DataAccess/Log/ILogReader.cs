using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogQuery.DataAccess.Log
{
    public interface ILogReader
    {
        IEnumerable<string> Lines { get; }
    }
}
