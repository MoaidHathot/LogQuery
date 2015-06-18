using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogQuery.DataAccess.Log
{
    public class LogLineContext
    {
        public string LogFile { get; set; }
        public string Message { get; set; }
        public long LogFileLine { get; set; }
        public long GlobalFileLine { get; set; }

        public LogLineContext()
        {

        }

        public override bool Equals(object obj)
        {
            var casted = obj as LogLineContext;

            if (null == casted)
            {
                return false;
            }

            return this.LogFile.Equals(casted.LogFile) && this.LogFileLine == casted.LogFileLine;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}", LogFile, LogFileLine).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("LogLineContext[Line: '{0}', Message: '{1}', LogFile: '{2}']", LogFileLine, Message, LogFile);
        }
    }
}
