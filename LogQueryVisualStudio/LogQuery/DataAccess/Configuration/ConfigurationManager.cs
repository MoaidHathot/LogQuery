using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    public class ConfigurationManager<T> : IConfigurationManager<T>
    {
        private IConfigurationSerializer<T> _serializer;

        public ConfigurationManager(IConfigurationSerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public void Export(string path, T content)
        {
            var xml = _serializer.Serialize(content);

            System.IO.File.WriteAllText(path, xml);
        }

        public T Import(string path)
        {
            var xml = System.IO.File.ReadAllText(path);

            return _serializer.Deserialize(xml);
        }
    }
}
