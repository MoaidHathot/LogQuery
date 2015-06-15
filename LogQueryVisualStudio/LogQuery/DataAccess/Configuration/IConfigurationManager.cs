using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    public interface IConfigurationManager<T>
    {
        void Export(string path, T content);
        T Import(string path);
    }
}
