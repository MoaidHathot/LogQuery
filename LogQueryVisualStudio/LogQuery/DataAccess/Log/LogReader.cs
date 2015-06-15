using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogQuery.DataAccess.Log
{
    public class LogReader : ILogReader
    {
        public string[] Files { get; set; }

        public LogReader(params string[] files)
        {
            this.Files = files;
        }

        public IEnumerable<string> Lines
        {
            get
            {
                foreach (var file in Files)
                {
                    foreach (var line in System.IO.File.ReadAllLines(file))
                    {
                        yield return line;
                    }
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
