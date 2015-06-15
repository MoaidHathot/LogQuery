using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    public class LogPattern
    {

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("RegularExpression")]
        public string RegularExpression { get; set; }

        [XmlElement("Members")]
        public LogPatternMember[] Members { get; set; }

        private LogPattern()
        {

        }

        public LogPattern(string name, string expression, params LogPatternMember[] members)
        {
            this.Name = name;
            this.RegularExpression = expression;

            this.Members = members;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LogPattern;

            if (other == null)
            {
                return false;
            }

            return 0 == string.Compare(this.Name, other.Name, true) && 0 == string.Compare(this.RegularExpression, other.RegularExpression, false);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}", Name).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("LogPattern[Name: '{0}', RegularExpression: '{1}', MemberCount: '{2}']", Name, RegularExpression, Members.Length);
        }
    }
}
