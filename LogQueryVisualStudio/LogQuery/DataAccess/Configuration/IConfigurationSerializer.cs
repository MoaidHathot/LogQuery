using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    public interface IConfigurationSerializer<T>
    {
        string Serialize(T configuration);
        T Deserialize(string content);
    }
}
