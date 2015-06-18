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

            //using (var stringWriter = new StringWriter())
            //{
            //    var settings = new XmlWriterSettings();
            //    settings.Indent = true;
            //    settings.Encoding = Encoding.UTF8;

            //    using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            //    {
            //        serializer.Serialize(xmlWriter, configuration);

            //        return stringWriter.ToString();
            //    }
            //}

            using (var memoryStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;
                settings.IndentChars = "\t";
                settings.NewLineChars = Environment.NewLine;
                settings.ConformanceLevel = ConformanceLevel.Document;

                using (var writer = XmlTextWriter.Create(memoryStream, settings))
                {
                    serializer.Serialize(writer, configuration);

                    return Encoding.UTF8.GetString(memoryStream.ToArray());
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
