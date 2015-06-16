using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LogQuery.DataAccess.Configuration
{
    public class ConfigurationXmlSerializer<T> : IConfigurationSerializer<T>
    {
        public string Serialize(T configuration)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;

                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, configuration);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize(string xmlContent)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stringReader = new StringReader(xmlContent))
            {
                var settings = new XmlReaderSettings();

                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }
    }
}
