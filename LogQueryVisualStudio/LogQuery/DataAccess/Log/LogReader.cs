using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogQuery.DataAccess.Log
{
    public class LogReader : ILogReader
    {
        public string[] Files { get; set; }
        public string Delimiter { get; set; }

        public LogReader(params string[] files)
            : this(Environment.NewLine, files)
        {

        }

        public LogReader(string delimiter, string[] files)
        {
            this.Files = files;
            this.Delimiter = delimiter;
        }

        public IEnumerable<LogLineContext> Lines
        {
            get
            {
                var globalLine = 0L;

                foreach (var file in Files)
                {
                    var text = System.IO.File.ReadAllText(file);

                    var logLine = 0L;

                    foreach (var line in text.Split(Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        yield return new LogLineContext { Message = line, LogFile = file, LogFileLine = logLine++, GlobalFileLine = globalLine++ };
                    }
                    //foreach (var line in System.IO.File.ReadAllLines(file))
                    //{
                    //    yield return line;
                    //}
                }
            }
        }

        public static LogReader FromDirectory(string directoryName)
        {
            var files = System.IO.Directory.GetFiles(directoryName).ToList();

            var infos = files.Select(f => new System.IO.FileInfo(f));
            var orderedInfos = infos.OrderBy(info => info.LastWriteTime);


            return new LogReader(orderedInfos.Select(info => info.FullName).ToArray());
        }
    }
}
