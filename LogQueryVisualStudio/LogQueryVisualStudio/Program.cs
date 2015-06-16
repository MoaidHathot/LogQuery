using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
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

            //var text = System.IO.File.ReadAllText(@"D:\Tinkoff\Test\Snippet.lqc");
            //var text = System.IO.File.ReadLines(@"D:\Tinkoff\Test\Snippet.lqc").ToArray();

            //var lines = text.Split(("\u0003").ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select( l => l.TrimStart());
            
            //Console.WriteLine("Press enter to quit.");
            //Console.ReadLine();
        }

        static void Test()
        {
            var configurationPath = @"D:\Tinkoff\Test\LogConfiguration.lqc";
            var logsDirectory = @"D:\Tinkoff\Test";

            var allWatch = Stopwatch.StartNew();

            Console.WriteLine("Exporting data...");
            var watch = Stopwatch.StartNew();
            var manager = new ConfigurationManager<LogPatternConfiguration>(new ConfigurationXmlSerializer<LogPatternConfiguration>());
            var exported = GetTestPatternConfiguartion(LogConstants.EndOfTextCharString);
            manager.Export(configurationPath, exported);
            watch.Stop();
            Console.WriteLine("Data is exported. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Importing data...");
            watch.Restart();
            var imported = manager.Import(configurationPath);
            //var imported = GetTestPatternConfiguartion();
            watch.Stop();
            Console.WriteLine("Data is imported. Elapsed: {0}", watch.Elapsed);

            //Console.WriteLine("Creating dataset...");
            //watch.Restart();
            //var set = CreateDataSet(imported);
            //watch.Stop();
            //Console.WriteLine("Dataset is created. Elapsed: {0}", watch.Elapsed);

            //Console.WriteLine("Generating creation query...");
            //watch.Restart();
            //var queryGenerator = new SqlServerQueryGenerator();
            //var query = queryGenerator.GenerateCreateQuery(set: set, createTablesIfNotExists: false);
            //watch.Stop();
            //Console.WriteLine("Query is generated. Elapsed: {0}", watch.Elapsed);

            //Console.WriteLine("Creating schema...");
            //watch.Restart();
            //var driver = new ManualDatabaseDriver(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\LogDB.mdf;Integrated Security=True");
            //driver.CretaeSchema(set);
            //watch.Stop();
            //Console.WriteLine("Schema is created. Elapsed: {0}", watch.Elapsed);

            //var reader = LogReader.FromDirectory(logsDirectory);
            //var parser = new LogParser(reader, imported);

            //Console.WriteLine("Parsing log...");
            //watch.Restart();
            //var parsedResults = parser.ParseLog();
            //watch.Stop();
            //Console.WriteLine("Log is parsed. Elapsed: {0}", watch.Elapsed);

            //Console.WriteLine("Structuring results...");
            //watch.Restart();
            //FillDataSet(set, parsedResults);
            //watch.Stop();
            //Console.WriteLine("Results are structured. Elapsed: {0}", watch.Elapsed);

            //Console.WriteLine("Saving results to db...");
            //watch.Restart();
            //driver.Save(set);
            //watch.Stop();
            //Console.WriteLine("results are saved to db. Elapsed: {0}", watch.Elapsed);

            allWatch.Stop();
            Console.WriteLine("Finished. Elapsed: {0}", allWatch.Elapsed);

            //PrintStatistics(set);
        }

        static void PrintStatistics(DataSet set)
        {
            foreach (DataTable table in set.Tables)
            {
                Console.WriteLine("Table {0} with '{1}' rows", table.TableName, table.Rows.Count);
            }
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

        static LogPatternConfiguration GetTestPatternConfiguartion(string delimiter)
        {
            return new LogPatternConfiguration("TestConfiguraiton", delimiter,

                new[]{

                    new LogPattern(
                        "StartedInteractions",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+CompleteDetails\.StartSegment: ---Start Segment\. \(segmentID = ([0-9]+), completeID = ([0-9]+)\)",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "InteractionID", typeof(long)), 
                            new LogPatternMember(4, "ContactID", typeof(long))
                        }
                    ),
                    new LogPattern(
                        "UpdateAuthenticationLevel",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.UpdateAuthenticationLevel: --->>\[UpdateAuthenticationLevel\(Level: '([a-zA-Z]+)', info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+''\)\]\[User='UserID: '([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "Level", typeof(string)), 
                            new LogPatternMember(4, "CompleteID", typeof(long)), 
                            new LogPatternMember(5, "InteractionID", typeof(long)),
                            new LogPatternMember(6, "User", typeof(int))
                        }
                    ),
                    new LogPattern(
                        "StartAuthenticationRequestStarted",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.StartAuthentication: --->>\[StartAuthentication\(info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+'', CustomerID='([0-9]+)'\)\]\[User='UserID: '([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "CompleteID", typeof(long)), 
                            new LogPatternMember(4, "InteractionID", typeof(long)),
                            new LogPatternMember(5, "CustomerID", typeof(long)), 
                            new LogPatternMember(6, "User", typeof(int))
                        }
                    ),
                    new LogPattern(
                        "StartAuthenticationRequestEnded",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.StartAuthentication: ---<<\[StartAuthentication\(info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+'', CustomerId='([0-9]+)'\)\]\[User='UserID: '([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "CompleteID", typeof(long)), 
                            new LogPatternMember(4, "InteractionID", typeof(long)),
                            new LogPatternMember(5, "CustomerID", typeof(long)), 
                            new LogPatternMember(6, "User", typeof(int))
                        }
                    )
                }
            );
        }
    }
}
