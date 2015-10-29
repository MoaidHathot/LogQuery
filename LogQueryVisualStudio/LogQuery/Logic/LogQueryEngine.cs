using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using LogQuery.DataAccess.Log;
using LogQuery.DataAccess.Database;
using LogQuery.DataAccess.Configuration;
using LogQuery.Properties;
using System.IO;
using System.Reflection;

namespace LogQuery.Logic
{
    public class LogQueryEngine : ILogQueryEngine
    {
        private string _name;
        private string[] _logFiles;
        private string[] _logConfigurationFiles;
        private IDatabaseDriver _dbDriver;

        protected string[] LogConfigurationFiles { get { return _logConfigurationFiles; } }
        protected IDatabaseDriver DatabaseDriver { get { return _dbDriver; } }
        protected string Name { get { return _name; } }
        public string OutputDirectory { get; set; }
        public string DatabaseFullName { get; set; }

        protected string[] LogFiles { get { return _logFiles; } }

        public LogQueryEngine(string name, string connectionString, string[] logFiles, string[] logConfiguraitonFiles)
        {
            _name = name;
            _dbDriver = new ManualDatabaseDriver(connectionString);
            _logFiles = logFiles;
            _logConfigurationFiles = logConfiguraitonFiles;
        }

        public virtual void Start()
        {
            var configuration = GetLogConfiguraitons(_logConfigurationFiles);

            var set = CreateDataSet(_name, configuration);

            CreateSchema(set);

            var results = ParseLogs(_logFiles, configuration);
            FillDataSet(set, results);

            SaveToDatabase(set);
        }

        protected virtual void SaveToDatabase(DataSet set)
        {
            _dbDriver.Save(set);
        }

        protected virtual object AdaptActualType(KeyValuePair<LogPatternMember, string> pair)
        {
            if (pair.Key.Type.Equals(typeof(int)))
            {
                return int.Parse(pair.Value);
            }
            else if (pair.Key.Type.Equals(typeof(DateTime)))
            {
                return DateTime.ParseExact(pair.Value, "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (pair.Key.Type.Equals(typeof(string)))
            {
                return pair.Value;
            }
            if (pair.Key.Type.Equals(typeof(long)))
            {
                return long.Parse(pair.Value);
            }
            if (pair.Key.Type.Equals(typeof(bool)))
            {
                return bool.Parse(pair.Value);
            }

            return pair.Value;
        }

        protected virtual void FillDataSet(DataSet set, Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> parsedResults)
        {
            var uniqueId = 0;

            foreach (var result in parsedResults)
            {
                var table = set.Tables[result.Key.Name];

                foreach (var pairs in result.Value)
                {
                    var list = new List<object>(pairs.Value.Select(p => AdaptActualType(p)).ToArray());
                    list.Add(--uniqueId);
                    list.Add(pairs.Key.LogFile);
                    list.Add(pairs.Key.GlobalFileLine);
                    list.Add(pairs.Key.LogFileLine);

                    table.Rows.Add(list.ToArray());
                }
            }
        }



        protected virtual Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> ParseLogs(string[] logFiles, IEnumerable<LogPatternConfiguration> configurations)
        {
            var reader = new LogReader(logFiles);
            var parser = new LogParser(reader, configurations);

            var parsedResults = parser.ParseLog();

            return parsedResults;
        }

 

        protected virtual void CreateSchema(DataSet set)
        {
            OutputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            DatabaseFullName = Path.Combine(OutputDirectory, set.Namespace + ".mdf");

            _dbDriver.CretaeSchema(DatabaseFullName, set);
        }

        protected virtual DataSet CreateDataSet(string name, IEnumerable<LogPatternConfiguration> configuraitons)
        {
            var set = new DataSet(name);

            //set.Namespace = set.DataSetName + "_" + DateTime.Now.ToString().Replace('/', '_').Replace(':', '_').Replace(" ", "_");
            set.Namespace = set.DataSetName + "_" + DateTime.Now.ToString().Replace('/', '_').Replace(':', '_').Replace(" ", "_");

            foreach (var configuration in configuraitons)
            {
                foreach (var pattern in configuration.Patterns)
                {
                    var table = set.Tables.Add(pattern.Name);

                    table.Namespace = configuration.Name;

                    foreach (var member in pattern.Members)
                    {
                        var column = table.Columns.Add(member.Name, member.Type);
                        column.AllowDBNull = false;
                    }

                    var idColumn = table.Columns.Add("__LogQ_ID__", typeof(int));
                    idColumn.AllowDBNull = false;
                    idColumn.AutoIncrement = true;
                    idColumn.Unique = true;

                    var fileColumn = table.Columns.Add("__LogQ_File__", typeof(string));
                    fileColumn.AllowDBNull = false;
                    fileColumn.Unique = false;

                    var globalLineColumn = table.Columns.Add("__LogQ_GlobalLineIndex__", typeof(long));
                    globalLineColumn.AllowDBNull = false;
                    globalLineColumn.Unique = true;

                    var lineColumn = table.Columns.Add("__LogQ_LineIndex__", typeof(long));
                    lineColumn.AllowDBNull = false;
                    lineColumn.Unique = true;
                }
            }

            return set;
        }

        protected virtual IEnumerable<LogPatternConfiguration> GetLogConfiguraitons(IEnumerable<string> configuraitonPaths)
        {
            var configurations = new List<LogPatternConfiguration>();

            var manager = new ConfigurationManager<LogPatternConfiguration>(new ConfigurationXmlSerializer<LogPatternConfiguration>());

            foreach (var path in configuraitonPaths)
            {
                configurations.Add(manager.Import(path));
            }

            return configurations;
        }

       
    }
}
