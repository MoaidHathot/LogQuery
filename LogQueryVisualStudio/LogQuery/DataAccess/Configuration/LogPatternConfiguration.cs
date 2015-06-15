using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    [XmlRoot]
    public class LogPatternConfiguration
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Patterns")]
        public LogPattern[] Patterns { get; set; }

        private LogPatternConfiguration()
        {

        }

        public LogPatternConfiguration(string name, params LogPattern[] patterns)
        {
            this.Name = name;
            this.Patterns = patterns;
        }
    }
}
