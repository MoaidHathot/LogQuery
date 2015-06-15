using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LogQuery;
using System.Data;
using LogQuery.DataAccess;
using LogQuery.DataAccess.Database;
using LogQuery.DataAccess.Configuration;
using LogQuery.DataAccess.Log;
using LogQuery.Properties;

namespace LogQueryVisualStudio
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
            
            Console.WriteLine("Press enter to quit.");
            Console.ReadLine();
        }

        static void Test()
        {
            var configurationPath = @"E:\Files\logs\LogQuery\LogConfiguration.lqc";
            var logsDirectory = @"E:\Files\logs\LogQuery";

            var manager = new ConfigurationManager<LogPatternConfiguration>(new ConfigurationXmlSerializer<LogPatternConfiguration>());
            manager.Export(configurationPath, GetTestPatternConfiguartion());
            var imported = manager.Import(configurationPath);

            var set = CreateDataSet(imported);

            var queryGenerator = new SqlServerQueryGenerator();
            var query = queryGenerator.GenerateCreateQuery(set: set, createTablesIfNotExists: false);

            Console.WriteLine("data is imported");

            var driver = new ManualDatabaseDriver(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\LogDB.mdf;Integrated Security=True");

            Console.WriteLine("Creating schema...");
            driver.CretaeSchema(set);

            var reader = LogReader.FromDirectory(logsDirectory);
            var parser = new LogParser(reader, imported);

            Console.WriteLine("Parsing log...");
            var parsedResults = parser.ParseLog();

            Console.WriteLine("Structuring results...");
            FillDataSet(set, parsedResults);

            Console.WriteLine("Saving results to db...");
            driver.Save(set);

            Console.WriteLine("Finished!");
        }

        static void FillDataSet(DataSet set, Dictionary<LogPattern, List<List<KeyValuePair<LogPatternMember, string>>>> results)
        {
            var uniqueId = 0;

            foreach (var result in results)
            {
                var table = set.Tables[result.Key.Name];

                foreach (var pairs in result.Value)
                {
                    var list = new List<object>(pairs.Select(p => AdaptActualType(p)).ToArray());
                    list.Add(--uniqueId);

                    table.Rows.Add(list.ToArray());
                }
            }
        }

        static object AdaptActualType(KeyValuePair<LogPatternMember, string> pair)
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

        static DataSet CreateDataSet(LogPatternConfiguration configuration)
        {
            DataSet set = new DataSet(configuration.Name);
            set.Namespace = "LogQuery_" + set.DataSetName + "_" + DateTime.Now.ToString().Replace('/', '_').Replace(':', '_').Replace(" ", "_");

            foreach (var pattern in configuration.Patterns)
            {
                var table = set.Tables.Add(pattern.Name);

                table.Namespace = configuration.Name;

                foreach (var member in pattern.Members)
                {
                    var column = table.Columns.Add(member.Name, member.Type);
                    column.AllowDBNull = false;
                }

                var idColumn = table.Columns.Add("__LogQueryID__", typeof(int));
                idColumn.AllowDBNull = false;
                idColumn.AutoIncrement = true;
                idColumn.Unique = true;
            }

            return set;
        }

        static LogPatternConfiguration GetTestPatternConfiguartion()
        {
            return new LogPatternConfiguration("TestConfiguraiton",

                new[]{

                    new LogPattern(
                        "StartedInteractions",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+[0-9a-zA-Z,# ]+\s+\|\s+RTIConnect\s*\|\s+CompleteDetails\.StartSegment: ---Start Segment\. \(segmentID = ([0-9]+), completeID = ([0-9]+)\)",

                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "InteractionID", typeof(long)), 
                            new LogPatternMember(3, "ContactID", typeof(long))
                        }
                    //),
                    //new LogPattern(
                    //    "UpdateAuthenticationLevel",
                    //    @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+[0-9a-zA-Z,# ]+\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.UpdateAuthenticationLevel: --->>\[UpdateAuthenticationLevel\(Level: '([a-zA-Z]+)', info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+''\)\]\[User='UserID: '([0-9]+)'",

                    //    new []{ 
                    //        new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                    //        new LogPatternMember(3, "Level", typeof(string)), 
                    //        new LogPatternMember(4, "CompleteID", typeof(long)), 
                    //        new LogPatternMember(5, "InteractionID", typeof(long)),
                    //        new LogPatternMember(6, "User", typeof(int))
                    //    }
                    )
                }
            );
        }
    }
}
