using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace LogQuery.DataAccess.Configuration
{
    public class LogPatternMember
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Index")]
        public int Index { get; set; }

        [XmlIgnore]
        public Type Type { get; set; }

        [XmlAttribute("Type")]
        public string TypeName { get { return Type.FullName; } set { Type = Type.GetType(value); } }

        private LogPatternMember()
        {

        }

        public LogPatternMember(int index, string name, Type type)
            : this(name, index, type)
        {
        }

        public LogPatternMember(string name, int index, Type type)
        {
            this.Name = name;
            this.Index = index;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as LogPatternMember;

            if (null == casted)
            {
                return false;
            }

            return this.Name == casted.Name && this.Index == casted.Index && this.Type.Equals(casted.Type);
        }

        public override int GetHashCode()
        {
            return string.Format("{0} {1} {2}", Index, Name, Type).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("LogPatternMember[Index: '{0}', Name: '{1}', Type: '{2}']", Index, Name, Type.Name);
        }
    }
}
