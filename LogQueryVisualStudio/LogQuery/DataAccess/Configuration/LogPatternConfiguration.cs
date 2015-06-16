using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace LogQuery.DataAccess.Configuration
{
    [XmlRoot]
    public class LogPatternConfiguration
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Patterns")]
        public LogPattern[] Patterns { get; set; }

        [XmlIgnore]
        private string _delimiter;

        [XmlAttribute("Delimiter")]
        public string DelimiterSerializer { get { return _delimiter; } set { _delimiter = value; } }

        [XmlIgnore]
        public string Delimiter { get { return UnEscape(DelimiterSerializer); } set { DelimiterSerializer = value; } }

        private LogPatternConfiguration()
        {
        }

        public LogPatternConfiguration(string name, string delimiter, params LogPattern[] patterns)
        {
            this.Name = name;
            this.Patterns = patterns;
            this.Delimiter = Escape(delimiter);
        }

        private string Escape(string s)
        {
            //return s;
            return Regex.Escape(s);
        }

        private string UnEscape(string s)
        {
            return Regex.Unescape(s);
            //return s;
        }
    }
}
